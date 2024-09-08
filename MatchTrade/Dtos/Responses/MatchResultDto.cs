using System.Collections.Generic;

namespace MatchTrade.Dtos.Responses
{
    public class MatchResultDto
    {
        public ApplyResultDto Taker { get; set; }
        public List<TradeResultDto> TradeResults { get; set; }
    }
}