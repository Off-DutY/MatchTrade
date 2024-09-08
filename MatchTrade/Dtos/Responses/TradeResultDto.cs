using System;

namespace MatchTrade.Dtos.Responses
{
    public class TradeResultDto
    {
        /// <summary>
        /// taker 搓合系統 id
        /// </summary>
        public long TakerOrderId;

        /// <summary>
        /// taker 用戶 id
        /// </summary>
        public long TakerUserId;

        /// <summary>
        /// taker 訂單 id
        /// </summary>
        public string TakerSourceOrderId;

        /// <summary>
        /// taker 掛單方式
        /// </summary>
        public int TakerOrderType;

        /// <summary>
        /// maker 搓合系統 id
        /// </summary>
        public long MakerOrderId;

        /// <summary>
        /// maker 用戶 id
        /// </summary>
        public long MakerUserId;

        /// <summary>
        /// maker 訂單 id
        /// </summary>
        public string MakerSourceOrderId;

        /// <summary>
        /// maker 掛單方式
        /// </summary>
        public int MakerOrderType;

        /// <summary>
        /// 是否為買單
        /// </summary>
        public bool IsBuyOrder;

        /// <summary>
        /// 交易数量
        /// </summary>
        public decimal Number;

        /// <summary>
        /// 实际成交价
        /// </summary>
        public decimal Price;

        /// <summary>
        /// 成交总价
        /// </summary>
        public decimal Amount;

        /// <summary>
        /// 交易對，區分不同貨幣或是站台使用
        /// </summary>
        public int SymbolId;

        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime TradeTime;

        /// <summary>
        /// 搓合甲方站台ID
        /// </summary>
        public int TakerSiteId { get; set; }

        /// <summary>
        /// 搓合乙方站台ID
        /// </summary>
        public int MakerSiteId { get; set; }
    }
}