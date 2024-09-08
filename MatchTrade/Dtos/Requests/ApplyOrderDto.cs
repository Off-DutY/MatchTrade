namespace MatchTrade.Dtos.Requests
{
    public class ApplyOrderDto
    {
        /// <summary>
        /// 價錢
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Member = (1, 會員的掛單)
        /// MMPay = (2, 承兌商的掛單)
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 買/賣
        /// </summary>
        public bool IsBuyOrder { get; set; }

        /// <summary>
        /// 優先權，越小越大，預設10
        /// </summary>
        public int Priority { get; set; } = 10;

        /// <summary>
        /// 會員Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 交易對，區分不同貨幣或是站台可以使用
        /// </summary>
        public int SymbolId { get; set; }

        /// <summary>
        /// 交易來源的Id
        /// </summary>
        public string SourceOrderId { get; set; }

        /// <summary>
        /// 來源站台
        /// </summary>
        public int SourceSiteId { get; set; }

        /// <summary>
        /// 單筆交易最小額度
        /// </summary>
        public decimal MinPriceEachMatch { get; set; } = 0;
    }
}