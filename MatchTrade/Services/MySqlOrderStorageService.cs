using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.MatchTrade;
using Data.MatchTrade.Factories.Interfaces;
using Data.MatchTrade.Models;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;
using MatchTrade.Enums;
using MatchTrade.Services.Interface;
using MatchTrade.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Services
{
    public class MySqlOrderStorageService : IOrderStorageService
    {
        private readonly ILogger<MySqlOrderStorageService> _logger;
        private readonly IMatchTradeContextFactory _matchTradeContextFactory;
        private MatchTradeContext MatchTradeContext => _matchTradeContextFactory.GetInstance;
        private MatchTradeContext MatchTradeContextReadOnly => _matchTradeContextFactory.GetInstanceReadOnly;

        public MySqlOrderStorageService(ILogger<MySqlOrderStorageService> logger,
                                        IMatchTradeContextFactory matchTradeContextFactory)
        {
            _logger = logger;
            _matchTradeContextFactory = matchTradeContextFactory;
        }

        public void Clean()
        {
            MatchTradeContext.OrderPools.RemoveRange(MatchTradeContext.OrderPools);
            MatchTradeContext.SaveChanges();
        }

        public async Task<List<OrderPool>> GetMatchingPool()
        {
            return await MatchTradeContextReadOnly.OrderPools
                    .AsNoTracking()
                    .Where(r => MatchEngineUtility.CanTradeStates.Contains(r.State))
                    .ToListAsync();
        }

        public async Task<Dictionary<long, MatchOrderDto>> GetOrderFromPool(string orderBookKey, MatchOrderDto matchOrderDto, CancellationToken cancellationToken)
        {
            var memberCode = EnumOrderType.Member.GetCode();
            var mmPayCode = EnumOrderType.MMPay.GetCode();

            if (matchOrderDto.OrderType == mmPayCode)
            {
                _logger.LogTrace("承兌商只需要找會員的掛單");
                return await MatchTradeContext.OrderPools
                        .AsNoTracking()
                        .Where(r =>
                                r.IsFinished == false &&
                                MatchEngineUtility.CanTradeStates.Contains(r.State) &&
                                r.OrderBookKey == orderBookKey &&
                                r.OrderType == memberCode &&
                                r.NoDealAmount <= matchOrderDto.NoDealAmount &&
                                r.NoDealAmount >= matchOrderDto.MinPriceEachMatch &&
                                (r.SourceSiteId != matchOrderDto.SourceSiteId || r.UserId != matchOrderDto.UserId))
                        .Select(r => new MatchOrderDto(r))
                        .ToDictionaryAsync(r => r.Id, r => r, cancellationToken);
            }

            if (matchOrderDto.OrderType == memberCode)
            {
                _logger.LogTrace("會員只找承兌商的掛單");
                return await MatchTradeContext.OrderPools
                        .AsNoTracking()
                        .Where(r =>
                                r.IsFinished == false &&
                                MatchEngineUtility.CanTradeStates.Contains(r.State) &&
                                r.OrderBookKey == orderBookKey &&
                                r.OrderType == mmPayCode &&
                                r.NoDealAmount >= matchOrderDto.NoDealAmount &&
                                r.MinPriceEachMatch <= matchOrderDto.NoDealAmount &&
                                (r.SourceSiteId != matchOrderDto.SourceSiteId || r.UserId != matchOrderDto.UserId))
                        .Select(r => new MatchOrderDto(r))
                        .ToDictionaryAsync(r => r.Id, r => r, cancellationToken);
            }

            return new Dictionary<long, MatchOrderDto>();
        }

        public IEnumerable<long> GetOrderBookHead(Dictionary<long, MatchOrderDto> outOrderBook)
        {
            foreach (var orderId in outOrderBook.Values
                             .OrderBy(r => r.Priority)
                             .ThenBy(r => r.Id)
                             .Select(r => r.Id))
            {
                yield return orderId;
            }
        }


        public async Task<(bool isSuccess, MatchOrderDto taker)> AddToOrderBook(MatchOrderDto matchOrderDto, CancellationToken cancellationToken)
        {
            _logger.LogTrace("AddToOrderBook Start");
            
            if (cancellationToken.IsCancellationRequested)
            {
                return (false, matchOrderDto);
            }

            var orderBookKey = MatchEngineUtility.GetOrderBookKey(matchOrderDto);

            var orderDto = await GetOrderFromPoolBySourceOrderId(orderBookKey, matchOrderDto.SourceSiteId, matchOrderDto.SourceOrderId, true, cancellationToken);

            if (orderDto == null)
            {
                _logger.LogTrace("全新掛單");
                orderDto = new OrderPool()
                {
                    OrderBookKey = orderBookKey,
                    SourceOrderId = matchOrderDto.SourceOrderId,
                    UserId = matchOrderDto.UserId,
                    Price = matchOrderDto.Price,
                    Number = matchOrderDto.Number,
                    Amount = matchOrderDto.Amount,
                    IsBuyOrder = matchOrderDto.IsBuyOrder,
                    OrderType = matchOrderDto.OrderType,
                    SymbolId = matchOrderDto.SymbolId,
                    State = matchOrderDto.State,
                    DealNum = matchOrderDto.DealNum,
                    NoDealNum = matchOrderDto.NoDealNum,
                    DealAmount = matchOrderDto.DealAmount,
                    NoDealAmount = matchOrderDto.NoDealAmount,
                    CreateTime = matchOrderDto.CreateTime,
                    UpdateTime = matchOrderDto.UpdateTime,
                    Priority = matchOrderDto.Priority,
                    SourceSiteId = matchOrderDto.SourceSiteId,
                    MinPriceEachMatch = matchOrderDto.MinPriceEachMatch,
                    IsFinished = false,
                };
                MatchTradeContext.OrderPools.Add(orderDto);
                await SaveChanges();

                return (true, new MatchOrderDto(orderDto));
            }

            _logger.LogInformation("不能重複新增掛單");
            return (false, new MatchOrderDto(orderDto));
        }

        public async Task<MatchOrderDto> UpdateOrderBook(MatchOrderDto matchOrderDto)
        {
            var orderBookKey = MatchEngineUtility.GetOrderBookKey(matchOrderDto);
            _logger.LogTrace("更新掛單資訊[{OrderBookKey}]", orderBookKey);

            var orderDto = await GetOrderFromPoolById(orderBookKey, matchOrderDto.Id, false);
            orderDto.State = matchOrderDto.State;
            orderDto.DealNum = matchOrderDto.DealNum;
            orderDto.NoDealNum = matchOrderDto.NoDealNum;
            orderDto.DealAmount = matchOrderDto.DealAmount;
            orderDto.NoDealAmount = matchOrderDto.NoDealAmount;
            orderDto.UpdateTime = matchOrderDto.UpdateTime;
            if (orderDto.State == EnumOrderState.ALL_DEAL.GetCode())
            {
                orderDto.IsFinished = true;
            }

            await SaveChanges();
            return new MatchOrderDto(orderDto);
        }

        public async Task<OrderPool> GetOrderFromPoolById(string orderBookKey, long makerId, bool readOnly)
        {
            var tradeContext = readOnly
                    ? MatchTradeContextReadOnly
                    : MatchTradeContext;

            var tradeContextOrderPools = tradeContext.OrderPools.AsQueryable();
            if (readOnly)
            {
                tradeContextOrderPools = tradeContextOrderPools.AsNoTracking();
            }

            var firstOrDefaultAsync = await tradeContextOrderPools
                    .FirstOrDefaultAsync(r => r.OrderBookKey == orderBookKey && r.Id == makerId);

            if (readOnly == false && firstOrDefaultAsync is not null)
            {
                await tradeContext.Entry(firstOrDefaultAsync).ReloadAsync();
            }

            return firstOrDefaultAsync;
        }

        public async Task<OrderPool> GetOrderFromPoolBySourceOrderId(string orderBookKey, int sourceSiteId, string sourceOrderId, bool readOnly, CancellationToken cancellationToken)
        {
            var tradeContext = readOnly
                    ? MatchTradeContextReadOnly
                    : MatchTradeContext;

            var tradeContextOrderPools = tradeContext.OrderPools.AsQueryable();
            if (readOnly)
            {
                tradeContextOrderPools = tradeContextOrderPools.AsNoTracking();
            }

            return await tradeContextOrderPools
                    .FirstOrDefaultAsync(r =>
                            r.OrderBookKey == orderBookKey &&
                            r.SourceOrderId == sourceOrderId &&
                            r.SourceSiteId == sourceSiteId &&
                            MatchEngineUtility.CanTradeStates.Contains(r.State) &&
                            r.IsFinished == false, cancellationToken);
        }

        public async Task SaveChanges()
        {
            await MatchTradeContext.SaveChangesAsync();
        }

        public async Task AddTradeResults(List<TradeResultDto> matchResultTradeResults)
        {
            _logger.LogTrace("AddTradeResults Start Save");

            MatchTradeContext.TradeResults.AddRange(matchResultTradeResults.Select(r => new TradeResult
            {
                TakerOrderId = r.TakerOrderId,
                TakerUserId = r.TakerUserId,
                TakerSourceOrderId = r.TakerSourceOrderId,
                TakerOrderType = r.TakerOrderType,
                TakerSiteId = r.TakerSiteId,
                MakerOrderId = r.MakerOrderId,
                MakerUserId = r.MakerUserId,
                MakerSourceOrderId = r.MakerSourceOrderId,
                MakerOrderType = r.MakerOrderType,
                MakerSiteId = r.MakerSiteId,
                IsBuyOrder = r.IsBuyOrder,
                Number = r.Number,
                Price = r.Price,
                Amount = r.Amount,
                SymbolId = r.SymbolId,
                TradeTime = r.TradeTime,
            }));
            await SaveChanges();
            _logger.LogTrace("AddTradeResults End Save");
        }
    }
}