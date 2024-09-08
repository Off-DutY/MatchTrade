using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Responses;
using MatchTrade.Services.Interface;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Services
{
    public class MmPartnerOrderMatchService : OrderMatchService
    {
        private readonly ILogger<MmPartnerOrderMatchService> _logger;
        private readonly IMatchEngineService _matchEngineService;
        private readonly IOrderStorageService _orderStorageService;

        public MmPartnerOrderMatchService(ILogger<MmPartnerOrderMatchService> logger,
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
            return await _orderStorageService.UpdateOrderBook(matchOrderDto);
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
            return await _matchEngineService.Cancel(matchOrderDto);
        }
    }
}