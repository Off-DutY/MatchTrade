using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Data.MatchTrade.Models;
using CoreLibrary;
using CoreLibrary.Dto.BaseResponses;
using CoreLibrary.Interface;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.Requests;
using MatchTrade.Dtos.Responses;
using MatchTrade.Enums;
using MatchTrade.Factory;
using MatchTrade.Services.Interface;
using MatchTrade.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TradeController : ControllerBase
    {
        private readonly ILogger<TradeController> _logger;
        private readonly MatchStrategyFactory _matchStrategyFactory;
        private readonly IOrderStorageService _orderStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IRedisService _redisService;
        private readonly IApiResultService _apiResultService;
        private readonly ITimeService _timeService;
        private CancellationToken CancellationToken => _httpContextAccessor.HttpContext.RequestAborted;

        public TradeController(ILogger<TradeController> logger,
                               MatchStrategyFactory matchStrategyFactory,
                               IOrderStorageService orderStorageService,
                               IHttpContextAccessor httpContextAccessor,
                               IRedisService redisService,
                               IApiResultService apiResultService,
                               ITimeService timeService)
        {
            _logger = logger;
            _matchStrategyFactory = matchStrategyFactory;
            _orderStorageService = orderStorageService;
            _httpContextAccessor = httpContextAccessor;
            _redisService = redisService;
            _apiResultService = apiResultService;
            _timeService = timeService;
        }

        [HttpGet(nameof(GetMatchingPool))]
        public async Task<ApiResultDto<List<OrderPool>>> GetMatchingPool()
        {
            var matchOrders = await _orderStorageService.GetMatchingPool();
            return _apiResultService.Ok(matchOrders);
        }

        [HttpPost(nameof(Order))]
        public async Task<ApiResultDto<MatchResultDto>> Order(ApplyOrderDto applyOrderDto)
        {
            var enumOrderType = EnumOrderType.Of(applyOrderDto.OrderType);

            if (enumOrderType == null || enumOrderType != EnumOrderType.Member)
            {
                var errorMessage = $"未定義或是不符合的{nameof(EnumOrderType)}:{applyOrderDto.OrderType}";
                _logger.LogInformation(errorMessage);
                return _apiResultService.Error<MatchResultDto>(new Exception(errorMessage));
            }

            if (applyOrderDto.Price <= 0)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("掛單金額必須大於0"));
            }

            if (applyOrderDto.MinPriceEachMatch != 0 && applyOrderDto.MinPriceEachMatch < 1)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("限制最低金額 1 元"));
            }

            _logger.LogTrace("會員[{UserId}]申請掛單", applyOrderDto.UserId);

            var matchOrderDto = CreateMatchOrderDto(applyOrderDto, enumOrderType);
            var orderMatchService = _matchStrategyFactory.GetByOrderType(enumOrderType);
            var matchResult = await orderMatchService.Match(matchOrderDto, CancellationToken);

            if (matchResult.taker == null)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("重複掛單"));
            }

            return _apiResultService.Ok(new MatchResultDto()
            {
                Taker = new ApplyResultDto(matchResult.taker),
                TradeResults = matchResult.tradeResults,
            });
        }

        [HttpPost(nameof(MmPayOrder))]
        public async Task<ApiResultDto<MatchResultDto>> MmPayOrder(ApplyOrderDto mmPayApplyOrderDto)
        {
            var enumOrderType = EnumOrderType.Of(mmPayApplyOrderDto.OrderType);
            if (enumOrderType == null || enumOrderType != EnumOrderType.MMPay)
            {
                var errorMessage = $"未定义或是不符合的{nameof(EnumOrderType)}:{mmPayApplyOrderDto.OrderType}";
                _logger.LogInformation(errorMessage);
                return _apiResultService.Error<MatchResultDto>(new Exception(errorMessage));
            }

            if (mmPayApplyOrderDto.Price <= 0)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("挂单金额必须大于 0 元"));
            }

            if (mmPayApplyOrderDto.MinPriceEachMatch != 0 && mmPayApplyOrderDto.MinPriceEachMatch < 1)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("限制最低金额为 1 元"));
            }

            _logger.LogTrace("MM商使用者[{UserId}]申請掛單", mmPayApplyOrderDto.UserId);
            
            var matchOrderDto = CreateMatchOrderDto(mmPayApplyOrderDto, enumOrderType);
            var orderMatchService = _matchStrategyFactory.GetByOrderType(enumOrderType);
            var matchResult = await orderMatchService.Match(matchOrderDto, CancellationToken);
            if (matchResult.taker == null)
            {
                return _apiResultService.Error<MatchResultDto>(new Exception("重复挂单"));
            }

            return _apiResultService.Ok(new MatchResultDto()
            {
                Taker = new ApplyResultDto(matchResult.taker),
                TradeResults = matchResult.tradeResults,
            });
        }

        [HttpPost(nameof(CancelOrder))]
        public async Task<ApiResultDto<ApplyResultDto>> CancelOrder(CancelOrderDto cancelDto)
        {
            var enumOrderType = EnumOrderType.Of(cancelDto.OrderType);
            if (enumOrderType == null)
            {
                var errorMessage = $"未定义或是不符合的{nameof(EnumOrderType)}:{cancelDto.OrderType}";
                _logger.LogInformation(errorMessage);
                return _apiResultService.Error<ApplyResultDto>(new Exception(errorMessage));
            }

            
            _logger.LogTrace("取消掛單!!!");
            var cancelOrderDto = CreateCancelOrderDto(cancelDto, enumOrderType);
            var orderMatchService = _matchStrategyFactory.GetByOrderType(enumOrderType);

            var match = await orderMatchService.Cancel(cancelOrderDto);
            if (match == null)
            {
                return _apiResultService.Error<ApplyResultDto>(new Exception(cancelOrderDto.Id.ToString()));
            }

            return _apiResultService.Ok(new ApplyResultDto(match));
        }

        [HttpPost(nameof(TakingOrderResult))]
        public ApiResultDto<string> TakingOrderResult(MmOrderRespondDto mmOrderRespondDto)
        {
            var enumMakerAnswer = EnumMakerAnswer.Of(mmOrderRespondDto.OrderResult);
            if (enumMakerAnswer == null)
            {
                return _apiResultService.Error<string>(new Exception($"未定義的資料:{mmOrderRespondDto.OrderResult}"));
            }

            var redisKey = string.Format(MatchEngineUtility.MmOrderRespondRedisKey, mmOrderRespondDto.SiteId, mmOrderRespondDto.OrderId);

            if (enumMakerAnswer == EnumMakerAnswer.ACCEPT)
            {
                RedisHelper.SetStringValue(_redisService.NonePrefixDb, redisKey, mmOrderRespondDto.MainOrderId.ToString(), TimeSpan.FromSeconds(30));
            }
            else
            {
                RedisHelper.SetStringValue(_redisService.NonePrefixDb, redisKey, "-1", TimeSpan.FromSeconds(30));
            }
            
            return _apiResultService.Ok("Ok");
        }

        private MatchOrderDto CreateMatchOrderDto(ApplyOrderDto applyOrderDto, EnumOrderType enumOrderType)
        {
            const int singlePrice = 1;
            var noDealAmount = singlePrice * applyOrderDto.Price;
            var matchOrderDto = new MatchOrderDto()
            {
                Price = singlePrice,
                Number = applyOrderDto.Price,
                Amount = noDealAmount,
                Priority = applyOrderDto.Priority,
                CreateTime = _timeService.GetNow(),
                UserId = applyOrderDto.UserId,
                SourceOrderId = applyOrderDto.SourceOrderId,
                OrderType = enumOrderType.GetCode(),
                SymbolId = applyOrderDto.SymbolId,
                IsBuyOrder = applyOrderDto.IsBuyOrder,
                NoDealAmount = noDealAmount,
                NoDealNum = noDealAmount,
                State = EnumOrderState.REQUEST.GetCode(),
                SourceSiteId = applyOrderDto.SourceSiteId,
                MinPriceEachMatch = applyOrderDto.MinPriceEachMatch
            };
            return matchOrderDto;
        }

        private static MatchOrderDto CreateCancelOrderDto(CancelOrderDto cancelOrderDto, EnumOrderType enumOrderType)
        {
            var matchOrderDto = new MatchOrderDto()
            {
                Id = cancelOrderDto.Id,
                UserId = cancelOrderDto.UserId,
                SourceOrderId = cancelOrderDto.SourceOrderId,
                OrderType = enumOrderType.GetCode(),
                SymbolId = cancelOrderDto.SymbolId,
                IsBuyOrder = cancelOrderDto.IsBuyOrder,
                SourceSiteId = cancelOrderDto.SourceSiteId,
                State = EnumOrderState.CANCEL.GetCode(),
            };
            return matchOrderDto;
        }
    }
}