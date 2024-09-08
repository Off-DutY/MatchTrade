using System;

namespace Data.MatchTrade.Models
{
    public class TradeResult
    {
        public long Id { get; set; }

        /// <summary>
        /// taker order id
        /// </summary>
        public long TakerOrderId { get; set; }

        /// <summary>
        /// taker user id
        /// </summary>
        public long TakerUserId { get; set; }

        /// <summary>
        /// taker 訂單id
        /// </summary>
        public string TakerSourceOrderId { get; set; }

        /// <summary>
        /// taker 掛單方式
        /// </summary>
        public int TakerOrderType { get; set; }

        public int TakerSiteId { get; set; }

        /// <summary>
        /// maker order id
        /// </summary>
        public long MakerOrderId { get; set; }

        /// <summary>
        /// maker user id
        /// </summary>
        public long MakerUserId { get; set; }

        /// <summary>
        /// maker账户id
        /// </summary>
        public string MakerSourceOrderId { get; set; }

        public int MakerOrderType { get; set; }


        public int MakerSiteId { get; set; }

        /// <summary>
        /// 是否是出价单
        /// </summary>
        public bool IsBuyOrder { get; set; }

        /// <summary>
        /// 交易数量
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 实际成交价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 成交总价
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 交易对ID
        /// </summary>
        public int SymbolId { get; set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime TradeTime { get; set; }
    }
}