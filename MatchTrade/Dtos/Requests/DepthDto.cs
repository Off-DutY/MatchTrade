namespace MatchTrade.Dtos.Requests
{
    public class DepthDto
    {
        /// <summary>
        /// 單價
        /// </summary>
        public long Price { get; set; }
        /// <summary>
        /// 交易數量
        /// </summary>
        public long Number { get; set; }
        /// <summary>
        /// 累計
        /// </summary>
        public long Total { get; set; }
    }
}