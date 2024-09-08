using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;

namespace MatchTrade.Services.Interface
{
    public interface IOrderNotifyService
    {
        NotifyRuleDto GetRule(MatchOrderDto taker, CancellationToken cancellationToken);
        Task<NotifyMakerDto> WaitingMmAcceptOrderAsync(MatchOrderDto taker, Dictionary<string, NotifyMakerDto> notifyMakerDic, CancellationToken cancellationToken);
    }
}