namespace MatchTrade.Dtos.Requests
{
    public class MmOrderRespondDto
    {
        /// <summary>
        /// 站代號
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 接單單號
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// MM商取款/入款申請單ID
        /// </summary>
        public int MainOrderId { get; set; }

        /// <summary>
        /// 訂單狀態
        /// </summary>
        public int OrderResult { get; set; }
    }
}