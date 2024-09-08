using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;
using MatchTrade.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Services
{
    public abstract class OrderMatchService : IOrderMatchService
    {
        private readonly ILogger _logger;

        protected OrderMatchService(ILogger logger)
        {
            _logger = logger;
        }
        
        public async Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> Match(MatchOrderDto matchOrderDto, CancellationToken cancellationToken = new CancellationToken())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (null, null);
            }

            _logger.LogTrace("Step1: 新增掛單紀錄");
            var addBookResult = await BeforeTakerMatch(matchOrderDto, cancellationToken);

            if (addBookResult.isSuccess == false)
            {
                _logger.LogTrace("重複掛單，結束");
                return (null, null);
            }

            _logger.LogTrace("Step2: 操作对手盘及产生trade信息");
            var matchResult = await StartMatch(addBookResult.taker, cancellationToken);

            _logger.LogTrace("Step3: 操作当前订单盘口");
            var afterTakerMatch = await AfterTakerMatch(matchResult.taker);

            _logger.LogTrace("step4: 紀錄交易結果");
            await AfterTrade(matchResult.tradeResults);

            return (afterTakerMatch, matchResult.tradeResults);
        }

        public async Task<MatchOrderDto> Cancel(MatchOrderDto matchOrderDto)
        {
            return await CancelMatch(matchOrderDto);
        }

        /// <Summary>
        /// 撮合后
        /// 操作当前订单盘口
        ///
        /// @param matchOrder 撮合订单
        /// @return 撮合订单
        /// </Summary>
        protected abstract Task<(bool isSuccess, MatchOrderDto taker)> BeforeTakerMatch(MatchOrderDto matchOrderDto, CancellationToken cancellationToken);

        protected abstract Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> StartMatch(MatchOrderDto matchOrderDto, CancellationToken cancellationToken);

        /// <Summary>
        /// 撮合后
        /// 操作当前订单盘口
        ///
        /// @param matchOrder 撮合订单
        /// @return 撮合订单
        /// </Summary>
        protected abstract Task<MatchOrderDto> AfterTakerMatch(MatchOrderDto matchOrderDto);

        protected abstract Task AfterTrade(List<TradeResultDto> matchResultTradeResults);

        protected abstract Task<MatchOrderDto> CancelMatch(MatchOrderDto matchOrderDto);
    }
}