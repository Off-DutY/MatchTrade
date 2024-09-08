using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.MatchTrade.Models;
using CoreLibrary;
using CoreLibrary.Dto;
using CoreLibrary.Interface;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;
using MatchTrade.Enums;
using MatchTrade.Services.Interface;
using MatchTrade.Utilities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MatchTrade.Services
{
    public class MatchEngineService : IMatchEngineService
    {
        private readonly ILogger<MatchEngineService> _logger;
        private readonly IOrderStorageService _orderStorageService;
        private readonly IEtcdLockService _etcdLocker;
        private readonly IRedisService _redisService;
        private readonly IOrderNotifyService _orderNotifyService;
        private readonly ITimeService _timeService;

        public MatchEngineService(ILogger<MatchEngineService> logger, IOrderStorageService orderStorageService, IEtcdLockService etcdLocker,
                                  IRedisService redisService, IOrderNotifyService orderNotifyService, ITimeService timeService)
        {
            _logger = logger;
            _orderStorageService = orderStorageService;
            _etcdLocker = etcdLocker;
            _redisService = redisService;
            _orderNotifyService = orderNotifyService;
            _timeService = timeService;
        }

        /// <summary>
        /// 取消搓合
        /// </summary>
        /// <param name="taker"></param>
        /// <returns></returns>
        /// <exception cref="GetLockerFailedException">取得Locker失敗</exception>
        public async Task<MatchOrderDto> Cancel(MatchOrderDto taker)
        {
            try
            {
                var orderBookKey = MatchEngineUtility.GetOrderBookKey(taker);

                _logger.LogTrace("取消搓合紀錄 {OrderBookKey}", orderBookKey);
                var orderDto = await _orderStorageService.GetOrderFromPoolById(orderBookKey, taker.Id, true);
                if (orderDto == null)
                {
                    return null;
                }

                using (var locker = await _etcdLocker.Lock(_logger, "MatchTrade", taker.Id.ToString(), taker.OrderType.ToString(),
                           new EtcdLockOptionDto()
                           {
                               LockTime = 300,
                               MaxRetryCount = 15,
                               WaitConnectionTimeout = 4
                           }))
                {
                    if (locker.IsAcquired)
                    {
                        _logger.LogTrace("Locker 取得成功");
                        orderDto = await _orderStorageService.GetOrderFromPoolById(orderBookKey, taker.Id, false);
                        if (orderDto == null)
                        {
                            return null;
                        }

                        if (orderDto.IsFinished || orderDto.State == EnumOrderState.CANCEL.GetCode())
                        {
                            _logger.LogInformation("目標已經是取消狀態，不需要再更新");
                        }
                        else
                        {
                            _logger.LogTrace("更新狀態為取消");
                            orderDto.State = EnumOrderState.CANCEL.GetCode();
                            orderDto.UpdateTime = _timeService.GetNow();
                            orderDto.IsFinished = true;

                            await _orderStorageService.SaveChanges();
                        }

                        return new MatchOrderDto(orderDto);
                    }
                    else
                    {
                        throw new GetLockerFailedException();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelMatchError {ErrorMsg}", ex.Message);
                throw new GetLockerFailedException();
            }
        }

        /// <summary>
        /// 处理撮合
        /// </summary>
        /// <param name="taker"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> DoMatch(MatchOrderDto taker, CancellationToken cancellationToken)
        {
            var tradeResults = new List<TradeResultDto>();
            taker.State = EnumOrderState.ORDER.GetCode();
            taker.UpdateTime = _timeService.GetNow();

            if (cancellationToken.IsCancellationRequested)
            {
                return (taker, tradeResults);
            }

            var reOrderBookKey = MatchEngineUtility.GetReverseOrderBookKey(taker);

            _logger.LogTrace("DoMatch Start");
            try
            {
                var orderBook = await _orderStorageService.GetOrderFromPool(reOrderBookKey, taker, cancellationToken);
                if (orderBook.Count == 0)
                {
                    _logger.LogTrace("沒有符合的掛單");
                    return (taker, tradeResults);
                }

                _logger.LogTrace("符合的撮合掛單數量:{OrderBookCount}", orderBook.Count);

                // 取得搓合的規則
                var notifyRuleDto = _orderNotifyService.GetRule(taker, cancellationToken);

                var hasNotifyTimes = 0;

                while (true)
                {
                    // 要通知的MM商
                    var notifyMakerDic = new Dictionary<string, NotifyMakerDto>();
                    // etcdLock storage
                    var lockerDic = new List<ILock>();
                    // redisKey storage
                    var cacheRedisKeyDic = new List<string>();

                    if (hasNotifyTimes >= notifyRuleDto.SendTakingOrderTimes)
                    {
                        _logger.LogDebug("已達發送次數上限 {Times} 次", notifyRuleDto.SendTakingOrderTimes);
                        break;
                    }

                    try
                    {
                        foreach (var orderId in _orderStorageService.GetOrderBookHead(orderBook))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return (taker, tradeResults);
                            }

                            if (notifyMakerDic.Count >= notifyRuleDto.TakerPoolMaxLimit)
                            {
                                _logger.LogDebug("已達單次通知人數上限 {MaxLimit} 人", notifyRuleDto.TakerPoolMaxLimit);
                                break;
                            }

                            var maker = orderBook[orderId];

                            if (MatchEngineUtility.CanTrade(taker, maker) == false)
                            {
                                _logger.LogTrace("目標不符合撮合條件，下一位");
                                orderBook.Remove(orderId);
                                continue;
                            }

                            _logger.LogTrace("使用Redis確認目標是否為搓合中的單子");
                            var redisKey = $"MatchTrade:{maker.Id}:{maker.OrderType}";
                            if (RedisHelper.KeyExistsInCache(_redisService.NonePrefixDb, RedisType.String, redisKey))
                            {
                                _logger.LogTrace("目標已經是搓合中的單子{RedisKey}，下一位", redisKey);
                                orderBook.Remove(orderId);
                                continue;
                            }

                            var locker = await _etcdLocker.Lock(_logger, "MatchTrade", maker.Id.ToString(), maker.OrderType.ToString(), new EtcdLockOptionDto()
                            {
                                LockTime = 300,
                                MaxRetryCount = 1,
                                WaitConnectionTimeout = 2
                            });

                            if (locker.IsAcquired)
                            {
                                // 這邊不使用try+finally的時候呼叫Dispose
                                // 因為必須等到全部撮合完成才能Dispose，除非該筆不符合條件
                                _logger.LogTrace("Etcd Lock Success");
                                _logger.LogTrace("避免重複計算，先從dic移除");
                                orderBook.Remove(orderId);

                                _logger.LogTrace("redis 快取，加速其他人的判定而不用等Etcd lock fail");
                                RedisHelper.SetStringValue(_redisService.NonePrefixDb, redisKey, "1", TimeSpan.FromSeconds(300));

                                _logger.LogTrace("取得目標撮合的最新資料");
                                var matchOrder = await _orderStorageService.GetOrderFromPoolById(reOrderBookKey, maker.Id, false);

                                if (matchOrder == null || matchOrder.IsFinished)
                                {
                                    _logger.LogInformation("該筆是已完成的訂單，或是找不到單(雖然不可能)");
                                    RedisHelper.DeleteKey(_redisService.NonePrefixDb, redisKey);
                                    locker.Dispose();
                                    continue;
                                }

                                var matchMaker = new MatchOrderDto(matchOrder);

                                if (MatchEngineUtility.CanTrade(taker, matchMaker) == false)
                                {
                                    // 額度不足，略過
                                    RedisHelper.DeleteKey(_redisService.NonePrefixDb, redisKey);
                                    locker.Dispose();
                                    continue;
                                }

                                _logger.LogTrace("新增撮合通知資訊");
                                lockerDic.Add(locker);
                                cacheRedisKeyDic.Add(redisKey);
                                notifyMakerDic.Add(matchMaker.SourceOrderId, new NotifyMakerDto()
                                {
                                    MatchMakerDto = matchMaker,
                                    DbMatchOrderDto = matchOrder,
                                });
                            }
                            else
                            {
                                _logger.LogTrace("Etcd Lock Fail，代表有人使用中，下一位");
                                orderBook.Remove(orderId);
                                continue;
                            }
                        }

                        if (notifyMakerDic.Any())
                        {
                            hasNotifyTimes++;
                            _logger.LogTrace("第{HasNotifyTimes}輪通知人數:{NotifyMakerDicCount}", hasNotifyTimes, notifyMakerDic.Count);
                            var mmAcceptResult = await _orderNotifyService.WaitingMmAcceptOrderAsync(taker, notifyMakerDic, cancellationToken);

                            if (mmAcceptResult.MakerAnswer != EnumMakerAnswer.ACCEPT)
                            {
                                _logger.LogTrace("全部拒絕或者是沒有人接單子");
                                continue;
                            }

                            if (await DealOrder(taker, mmAcceptResult.MatchMakerDto, mmAcceptResult.DbMatchOrderDto, tradeResults))
                            {
                                _logger.LogInformation("搓合成功，{TakerSourceOrderId}", taker.SourceOrderId);
                                return (taker, tradeResults);
                            }
                        }
                        else
                        {
                            _logger.LogTrace("找不到可撮合的訂單");
                            return (taker, tradeResults);
                        }
                    }
                    finally
                    {
                        _logger.LogTrace("清空快取與Locker");
                        if (lockerDic.Any())
                        {
                            foreach (var locker in lockerDic)
                            {
                                locker.Dispose();
                            }
                        }

                        if (cacheRedisKeyDic.Any())
                        {
                            foreach (var redisKey in cacheRedisKeyDic)
                            {
                                RedisHelper.DeleteKey(_redisService.NonePrefixDb, redisKey);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "doMatchError {ErrorMsg}", ex.Message);
            }

            return (taker, tradeResults);
        }

        private async Task<bool> DealOrder(MatchOrderDto taker, MatchOrderDto matchMaker, OrderPool matchOrder, List<TradeResultDto> tradeResults)
        {
            var contrast = ComparerUtility.Comparer(taker.NoDealNum, matchMaker.NoDealNum);
            // 成交数量
            var dealNum = contrast < 0
                    ? taker.NoDealNum
                    : matchMaker.NoDealNum;

            MatchEngineUtility.MakerHandle(matchMaker, dealNum, contrast);

            var matchOrderUpdateTime = _timeService.GetNow();
            matchOrder.State = matchMaker.State;
            matchOrder.DealNum = matchMaker.DealNum;
            matchOrder.NoDealNum = matchMaker.NoDealNum;
            matchOrder.DealAmount = matchMaker.DealAmount;
            matchOrder.NoDealAmount = matchMaker.NoDealAmount;
            matchOrder.UpdateTime = matchOrderUpdateTime;

            if (matchOrder.State == EnumOrderState.ALL_DEAL.GetCode())
            {
                matchOrder.IsFinished = true;
            }

            await _orderStorageService.SaveChanges();

            var now = _timeService.GetNow();
            MatchEngineUtility.TakerHandle(taker, matchMaker.Price, dealNum, contrast, now);
            var tradeResultDto = new TradeResultDto
            {
                TakerOrderId = taker.Id,
                TakerUserId = taker.UserId,
                TakerSourceOrderId = taker.SourceOrderId,
                TakerOrderType = taker.OrderType,
                TakerSiteId = taker.SourceSiteId,
                MakerOrderId = matchOrder.Id,
                MakerUserId = matchOrder.UserId,
                MakerSourceOrderId = matchOrder.SourceOrderId,
                MakerOrderType = matchOrder.OrderType,
                MakerSiteId = matchOrder.SourceSiteId,
                IsBuyOrder = taker.IsBuyOrder,
                Number = dealNum,
                Price = matchOrder.Price,
                Amount = matchOrder.Price * dealNum,
                SymbolId = taker.SymbolId,
                TradeTime = matchOrderUpdateTime,
            };
            tradeResults.Add(tradeResultDto);

            // taker 是完成状态
            if (taker.State == EnumOrderState.ALL_DEAL.GetCode())
            {
                return true;
            }

            return false;
        }
    }
}