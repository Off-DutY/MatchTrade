using System;
using System.Collections.Generic;
using CoreLibrary;
using MatchTrade.Dtos.Messages;
using MatchTrade.Enums;

namespace MatchTrade.Utilities
{
    public class MatchEngineUtility
    {
        /// <summary>
        /// Respond RedisKey string format
        /// MatchTrade:MmOrderRespond:{SiteId}:{OrderId}
        /// Value will be {MainOrderId} or -1
        /// -1 mean nobody accept order
        /// if value great than 0
        /// value mean MmPayBoardId
        /// </summary>
        public static string MmOrderRespondRedisKey = "MatchTrade:MmOrderRespond:{0}:{1}";
        /// <Summary>
        /// 买订单簿BID
        /// </Summary>
        private static string _orderBookBidMap = "BOOK-BID-";

        /// <Summary>
        /// 卖订单簿ASK
        /// </Summary>
        private static string _orderBookAskMap = "BOOK-ASK-";

        /// <Summary>
        /// 卖订单簿ASK
        /// </Summary>
        private static string _orderDeWeight = "ORDER-DE-WEIGHT-";

        /// <Summary>
        /// 会自动撤销的
        /// </Summary>
        private static List<EnumOrderType> _autoCancel = new List<EnumOrderType>();

        public static List<int> CanTradeStates = new List<int>()
        {
            EnumOrderState.ORDER.GetCode(),
            EnumOrderState.SOME_DEAL.GetCode(),
        };

        /// <Symmary>
        /// 是否是出价单 是 卖订单簿ASK 否 买订单簿BID
        /// 对手订单簿订单key获取
        /// </Symmary>
        public static string GetReverseOrderBookKey(MatchOrderDto orderDto)
        {
            return GetOrderBookKey(orderDto.IsBuyOrder == false, orderDto.SymbolId);
        }

        /// <Symmary>
        /// 是否是出价单 是 买订单簿BID 否 卖订单簿ASK
        /// 订单簿订单key获取
        ///
        /// @param order 撮合订单
        /// @return string 订单簿订单key
        /// </Symmary>
        public static string GetOrderBookKey(MatchOrderDto orderDto)
        {
            return GetOrderBookKey(orderDto.IsBuyOrder, orderDto.SymbolId);
        }

        /// <Symmary>
        /// 是否是出价单 是 买订单簿BID 否 卖订单簿ASK
        /// 订单簿订单key获取
        ///
        /// @param symbol 交易币对
        /// @param ifBid  买卖标识
        /// @return string 订单簿订单key
        /// </Symmary>
        private static string GetOrderBookKey(bool ifBid, long symbol)
        {
            return (ifBid
                    ? _orderBookBidMap
                    : _orderBookAskMap) + symbol;
        }

        /// <Symmary>
        /// 判断是否可以交易
        /// </Symmary>
        public static bool CanTrade(MatchOrderDto taker, MatchOrderDto maker)
        {
            // 承兌商使用總額計算
            if (EnumOrderType.Of(taker.OrderType) == EnumOrderType.MMPay &&
                EnumOrderType.Of(maker.OrderType) == EnumOrderType.Member)
            {
                return taker.NoDealAmount >= maker.NoDealAmount;
            }

            if (EnumOrderType.Of(taker.OrderType) == EnumOrderType.Member &&
                EnumOrderType.Of(maker.OrderType) == EnumOrderType.MMPay)
            {
                return taker.NoDealAmount <= maker.NoDealAmount;
            }

            return false;
        }

        /// <Symmary>
        ///  处理taker撮合结果
        ///
        /// @param taker   计价单
        /// @param maker   交易单 
        /// @param contrast taker对比maker的数量
        /// @param dealNum 交易数量
        /// @param dealNum 当前操作数量
        /// @return
        /// </Symmary>
        public static void TakerHandle(MatchOrderDto taker, decimal dealPrice, decimal dealNum, int contrast, DateTime now)
        {
            if (contrast > 0)
            {
                // taker 有余
                taker.State = EnumOrderState.SOME_DEAL.GetCode();
            }
            else if (contrast == 0)
            {
                // 都无剩余
                taker.State = EnumOrderState.ALL_DEAL.GetCode();
                // 发送maker，和trade
            }
            else
            {
                // maker有剩余
                taker.State = EnumOrderState.ALL_DEAL.GetCode();
                // 发送maker，和trade
            }

            // calculate taker
            taker.DealNum += dealNum;
            taker.NoDealNum -= dealNum;
            taker.DealAmount += dealPrice * dealNum;
            taker.NoDealAmount -= dealPrice * dealNum;
            taker.UpdateTime = now;
        }

        /// <Symmary>
        ///  处理maker撮合结果
        ///
        /// @param taker   计价单
        /// @param maker   交易单 
        /// @param contrast taker对比maker的数量
        /// @param dealNum 交易数量
        /// @param dealNum 当前操作数量
        /// @return
        /// </Symmary>
        public static void MakerHandle(MatchOrderDto maker, decimal dealNum, int contrast)
        {
            if (contrast > 0)
            {
                // taker 有余
                maker.State = EnumOrderState.ALL_DEAL.GetCode();
            }
            else if (contrast == 0)
            {
                // 都无剩余
                maker.State = EnumOrderState.ALL_DEAL.GetCode();
                // 发送maker，和trade
            }
            else
            {
                // maker有剩余
                maker.State = EnumOrderState.SOME_DEAL.GetCode();
                // 发送maker，和trade
            }

            // calculate maker
            maker.DealNum += dealNum;
            maker.NoDealNum -= dealNum;
            maker.DealAmount += maker.Price * dealNum;
            maker.NoDealAmount -= maker.Price * dealNum;
        }

        /// <Symmary>
        /// - 相同价格时取最优
        ///
        /// @param o1
        /// @param o2
        /// @return
        /// </Symmary>
        public static int ComparatorOptimal(MatchOrderDto o1, MatchOrderDto o2)
        {
            // 比优先级：因为优先级取最大，和卖相反
            int rsg = ComparerUtility.Comparer(o2.Priority, o1.Priority);
            // 优先级一样
            if (rsg == 0)
            {
                rsg = ComparerUtility.Comparer(o2.Id, o1.Id);
            }

            return rsg;
        }

        /// <Symmary>
        /// @Title: autoToCancel @Description: TODO(用于自动转撤销) @param orderType 参数 @return
        /// bool 返回类型 @throws
        /// </Symmary>
        public static bool AutoToCancel(EnumOrderType orderType)
        {
            if (_autoCancel.Contains(orderType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <Symmary>
        /// 订单接收消息check
        /// </Symmary>
        public static void CheckOrderMqInput(MatchOrderDto dto)
        {
            if (dto.State.In(EnumOrderState.ORDER.GetCode(), EnumOrderState.SOME_DEAL.GetCode()) == false)
            {
                throw new Exception("订单状态无法撮合");
            }

            if (EnumOrderType.Of(dto.OrderType) == null)
            {
                throw new Exception("订单类型不支持撮合");
            }

            // 限价
            if (dto.OrderType == EnumOrderType.Member.GetCode())
            {
                if (dto.NoDealNum > 0 && dto.NoDealNum < dto.Number == false)
                {
                    throw new Exception("未成交数量异常");
                }

                if (dto.NoDealAmount > 0 && dto.NoDealAmount < dto.Amount == false)
                {
                    throw new Exception("未成交总额异常");
                }
            }
        }
    }
}