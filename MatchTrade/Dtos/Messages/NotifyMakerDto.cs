using Data.MatchTrade.Models;
using MatchTrade.Enums;

namespace MatchTrade.Dtos.Messages
{
    public class NotifyMakerDto
    {
        public MatchOrderDto MatchMakerDto { get; set; }
        public OrderPool DbMatchOrderDto { get; set; }
        public EnumMakerAnswer MakerAnswer { get; set; }
    }
}