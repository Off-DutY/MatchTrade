using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;

namespace MatchTrade.Services.Interface
{
    public interface IMatchEngineService
    {
        Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> DoMatch(MatchOrderDto taker, CancellationToken cancellationToken);
        Task<MatchOrderDto> Cancel(MatchOrderDto taker);
    }
}