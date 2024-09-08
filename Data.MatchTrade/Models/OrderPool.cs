using System;

namespace Data.MatchTrade.Models
{
    /// <summary>
    /// 撮合訂單DTO
    /// </summary>
    public class OrderPool
    {
        /// <summary>
        /// Identity
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 來源交易ID
        /// </summary>
        public string SourceOrderId { get; set; }

        /// <summary>
        /// 用戶Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 單價
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 總額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 是否為買單
        /// </summary>
        public bool IsBuyOrder { get; set; }

        /// <summary>
        /// 訂單類型
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 交易對列
        /// </summary>
        public int SymbolId { get; set; }

        /// <summary>
        /// 狀態 - 參考 EnumOrderState
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 成交數量
        /// </summary>
        public decimal DealNum { get; set; }

        /// <summary>
        /// 未成交數量
        /// </summary>
        public decimal NoDealNum { get; set; }

        /// <summary>
        /// 成交總額
        /// </summary>
        public decimal DealAmount { get; set; }

        /// <summary>
        /// 未成交總額
        /// </summary>
        public decimal NoDealAmount { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 交易簿的唯一Key
        /// </summary>
        public string OrderBookKey { get; set; }

        /// <summary>
        /// 最小交易額度
        /// </summary>
        public decimal MinPriceEachMatch { get; set; }

        /// <summary>
        /// 掛單來源站台Id
        /// </summary>
        public int SourceSiteId { get; set; }

        /// <summary>
        /// 掛單是否已結束
        /// </summary>
        public bool IsFinished { get; set; }
    }
}