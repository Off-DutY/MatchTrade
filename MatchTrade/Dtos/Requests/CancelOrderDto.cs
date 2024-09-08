namespace MatchTrade.Dtos.Requests
{
    public class CancelOrderDto
    {
        public long Id { get; set; }
        public int OrderType { get; set; }
        public int SymbolId { get; set; }
        public int UserId { get; set; }
        public string SourceOrderId { get; set; }
        public bool IsBuyOrder { get; set; }
        public int SourceSiteId { get; set; }
    }
}