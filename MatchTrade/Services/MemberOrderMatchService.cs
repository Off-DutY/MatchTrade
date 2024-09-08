using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;
using MatchTrade.Enums;
using MatchTrade.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Services
{
    public class MemberOrderMatchService : OrderMatchService
    {
        private readonly ILogger<MemberOrderMatchService> _logger;
        private readonly IMatchEngineService _matchEngineService;
        private readonly IOrderStorageService _orderStorageService;

        public MemberOrderMatchService(ILogger<MemberOrderMatchService> logger,
                                       IMatchEngineService matchEngineService,
                                       IOrderStorageService orderStorageService)
                : base(logger)
        {
            _logger = logger;
            _matchEngineService = matchEngineService;
            _orderStorageService = orderStorageService;
        }

        protected override async Task<(bool isSuccess, MatchOrderDto taker)> BeforeTakerMatch(MatchOrderDto matchOrderDto, CancellationToken cancellationToken)
        {
            return await _orderStorageService.AddToOrderBook(matchOrderDto, cancellationToken);
        }

        protected override async Task<(MatchOrderDto taker, List<TradeResultDto> tradeResults)> StartMatch(MatchOrderDto matchOrderDto, CancellationToken cancellationToken)
        {
            return await _matchEngineService.DoMatch(matchOrderDto, cancellationToken);
        }

        protected override async Task<MatchOrderDto> AfterTakerMatch(MatchOrderDto matchOrderDto)
        {
            if (matchOrderDto.State == EnumOrderState.ALL_DEAL.GetCode())
            {
                _logger.LogTrace("搓合成功就更新並回傳");
                return await _orderStorageService.UpdateOrderBook(matchOrderDto);
            }

            _logger.LogTrace("搓合失敗就直接取消");
            return await _matchEngineService.Cancel(matchOrderDto);
        }

        protected override async Task AfterTrade(List<TradeResultDto> matchResultTradeResults)
        {
            if (matchResultTradeResults != null && matchResultTradeResults.Any())
            {
                _logger.LogTrace("搓合成功，紀錄詳細掛單資訊，{MatchResultTradeResultsCount}筆", matchResultTradeResults.Count);
                await _orderStorageService.AddTradeResults(matchResultTradeResults);
            }
        }

        protected override async Task<MatchOrderDto> CancelMatch(MatchOrderDto matchOrderDto)
        {
            _logger.LogCritical("絕對不會遇到，因為會員單只有成功或取消兩種狀態，不可能會有掛單中然後取消的情況");
            return await _matchEngineService.Cancel(matchOrderDto);
        }
    }
}