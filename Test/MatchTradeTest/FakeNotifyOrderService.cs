using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Enums;
using MatchTrade.Services.Interface;

namespace MatchTradeTest
{
    public class FakeNotifyOrderService : IOrderNotifyService
    {
        public Task<EnumMakerAnswer> WaitingMmAcceptOrderAsync(MatchOrderDto taker, MatchOrderDto maker, CancellationToken cancellationToken)
        {
            return Task.FromResult(EnumMakerAnswer.ACCEPT);
        }

        public NotifyRuleDto GetRule(MatchOrderDto taker, CancellationToken cancellationToken)
        {
            return new NotifyRuleDto
            {
                TakerPoolMaxLimit = 1,
                SendTakingOrderTimes = 1,
                TotalRespondWaitingTimeInMilliseconds = 1000,
                WaitingTimeBeforeCheckResponseInMilliseconds = 1000,
                IntervalTimeBetweenCheckRedisValueInMilliseconds = 1000,
            };
        }

        public Task<NotifyMakerDto> WaitingMmAcceptOrderAsync(MatchOrderDto taker, Dictionary<string, NotifyMakerDto> notifyMakerDic, CancellationToken cancellationToken)
        {
            var (key, value) = notifyMakerDic.First();
            value.MakerAnswer = EnumMakerAnswer.ACCEPT;
            return Task.FromResult(value);
        }
    }
}