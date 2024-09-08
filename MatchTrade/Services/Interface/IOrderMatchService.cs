using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;

namespace MatchTrade.Services.Interface
{
    public interface IOrderMatchService
    {
        /// <Summary>
        /// 撮合订单
        /// </Summary>
        Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> Match(MatchOrderDto matchOrderDto, CancellationToken cancellationToken);

        /// <summary>
        /// 取消撮合订单
        /// </summary>
        /// <param name="matchOrderDto"></param>
        /// <returns></returns>
        Task<MatchOrderDto> Cancel(MatchOrderDto matchOrderDto);
    }
}