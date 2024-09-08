using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoreLibrary;
using CoreLibrary.Interface;
using MatchTrade.Dtos.Messages;
using MatchTrade.Dtos.OrderNotify;
using MatchTrade.Dtos.OrderNotify.Responses;
using MatchTrade.Enums;
using MatchTrade.Services.Interface;
using MatchTrade.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Services
{
    public class OrderNotifyService : IOrderNotifyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;
        private readonly ILogger<OrderNotifyService> _logger;
        private HttpClient _httpClient;

        private static NotifyRuleDto NotifyRuleDto { get; set; }

        public HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = _httpClientFactory.CreateClient();
                }

                return _httpClient;
            }
        }

        public OrderNotifyService(IHttpClientFactory httpClientFactory,
                                  IConfiguration configuration,
                                  IRedisService redisService,
                                  ILogger<OrderNotifyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _redisService = redisService;
            _logger = logger;
        }

        public NotifyRuleDto GetRule(MatchOrderDto taker, CancellationToken cancellationToken)
        {
            if (NotifyRuleDto == null)
            {
                NotifyRuleDto = _configuration.GetSection("AppCenter:NotifyRule").Get<NotifyRuleDto>();
            }

            return NotifyRuleDto;
        }

        public async Task<NotifyMakerDto> WaitingMmAcceptOrderAsync(MatchOrderDto taker, Dictionary<string, NotifyMakerDto> notifyMakerDic, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new NotifyMakerDto()
                {
                    MakerAnswer = EnumMakerAnswer.TIMEOUT
                };
            }

            _logger.LogTrace("本次通知筆數:{NotifyMakerDicCount}", notifyMakerDic.Count);

            MatchOrderDto memberUser = taker;

            _logger.LogTrace("取得發送規則");
            var notifyRuleDto = GetRule(memberUser, cancellationToken);

            _logger.LogTrace("送出接單請求");
            var requestResult = await SendGroupTakingOrder(memberUser, notifyMakerDic, notifyRuleDto.TotalRespondWaitingTimeInMilliseconds, cancellationToken);

            if (requestResult == null)
            {
                _logger.LogError("沒有結果，代表呼叫API失敗");
                return new NotifyMakerDto()
                {
                    MakerAnswer = EnumMakerAnswer.TIMEOUT
                };
            }

            _logger.LogTrace("成功送出之後等待{WaitingTimeBeforeCheckResponseInMilliseconds}秒鐘，因為不會有秒接單的情況", notifyRuleDto.WaitingTimeBeforeCheckResponseInMilliseconds);
            Thread.Sleep(TimeSpan.FromMilliseconds(notifyRuleDto.WaitingTimeBeforeCheckResponseInMilliseconds));

            var mainOrderId = string.Empty;
            // 定時確認是否有回傳的資料
            var redisKey = string.Format(MatchEngineUtility.MmOrderRespondRedisKey, memberUser.SourceSiteId, memberUser.SourceOrderId);
            _logger.LogTrace("本輪的確認RedisKey={RedisKey}", redisKey);
            EnumMakerAnswer makerAnswer = null;
            using (_logger.BeginScope("[SpinWait]-RedisKey={RedisKey}", redisKey))
            {
                SpinWait.SpinUntil(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return true;
                        }

                        var redisValue = RedisHelper.GetString(_redisService.NonePrefixDb, redisKey);
                        if (string.IsNullOrWhiteSpace(redisValue))
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(notifyRuleDto.IntervalTimeBetweenCheckRedisValueInMilliseconds));
                            return false;
                        }

                        _logger.LogTrace("Redis Value = {RedisValue}", redisValue);

                        RedisHelper.DeleteKey(_redisService.NonePrefixDb, redisKey);
                        if (redisValue == "-1")
                        {
                            // -1 代表全拒絕或是timeout
                            makerAnswer = EnumMakerAnswer.TIMEOUT;
                        }
                        else
                        {
                            // 存的資料代表接單成功的MM商訂單Id
                            makerAnswer = EnumMakerAnswer.ACCEPT;
                            mainOrderId = redisValue;
                        }

                        return true;
                    },
                    TimeSpan.FromMilliseconds(
                        notifyRuleDto.TotalRespondWaitingTimeInMilliseconds -
                        notifyRuleDto.WaitingTimeBeforeCheckResponseInMilliseconds));
            }

            if (makerAnswer == null)
            {
                _logger.LogTrace("還是沒結果，最後一次確認Redis資料");
                var redisValue = RedisHelper.GetString(_redisService.NonePrefixDb, redisKey);
                _logger.LogTrace("Redis Value = {RedisValue}", redisValue);
                if (string.IsNullOrWhiteSpace(redisValue) == false)
                {
                    RedisHelper.DeleteKey(_redisService.NonePrefixDb, redisKey);
                    if (redisValue == "-1")
                    {
                        makerAnswer = EnumMakerAnswer.TIMEOUT;
                    }
                    else
                    {
                        mainOrderId = redisValue;
                        makerAnswer = EnumMakerAnswer.ACCEPT; //ParseEnumMakerAnswer(int.Parse(redisValue));
                    }
                }
            }

            if (makerAnswer == null)
            {
                _logger.LogInformation("超過時間還沒拿到回應，自己再打一次API確認");
                var confirmOrderResult = await CheckMakerRequestAnswer(memberUser.SourceSiteId, memberUser.SourceOrderId, cancellationToken);
                if (confirmOrderResult != null)
                {
                    makerAnswer = ParseEnumMakerAnswer(confirmOrderResult.Result.OrderResult);
                    _logger.LogInformation("接單狀態{MakerAnswerDesc}", makerAnswer.GetDesc());
                    if (makerAnswer == EnumMakerAnswer.ACCEPT)
                    {
                        mainOrderId = confirmOrderResult.Result.MainOrderId.ToString();
                    }
                }
            }

            if (makerAnswer == null || makerAnswer != EnumMakerAnswer.ACCEPT)
            {
                _logger.LogInformation("接單失敗，回傳結果{Result}，makerAnswer={MakerAnswerDesc}", EnumMakerAnswer.TIMEOUT.GetDesc(), makerAnswer?.GetDesc());
                return new NotifyMakerDto()
                {
                    MakerAnswer = EnumMakerAnswer.TIMEOUT
                };
            }

            _logger.LogInformation("接單成功回傳，MM商訂單Id = {Result}", mainOrderId);

            var winMaker = notifyMakerDic[mainOrderId];
            winMaker.MakerAnswer = makerAnswer;
            return winMaker;
        }

        private async Task<NotifyApiResultBaseDto<string>> SendGroupTakingOrder(MatchOrderDto memberUser, Dictionary<string, NotifyMakerDto> notifyMakerDic, int totalRespondWaitingTimeInMilliseconds,
                                                                                CancellationToken cancellationToken)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, GetApiUrl(EnumOrderNotifyApi.TAKINGORDER));

            var parameters = new Dictionary<string, object>();

            var groupTakingOrderMerchantInfos = notifyMakerDic.Values.Select(r => r.MatchMakerDto)
                    .GroupBy(r => r.SourceSiteId)
                    .Select(r => new GroupTakingOrderMerchantInfo()
                    {
                        MmSiteId = r.Key,
                        MerchantInfoList = r.Select(y => new MerchantInfoBase()
                        {
                            MmUserId = y.UserId,
                            MainOrderId = int.Parse(y.SourceOrderId)
                        }).ToList()
                    }).ToList();

            parameters.Add("TakingHistoryId", memberUser.SourceOrderId);
            parameters.Add("UserSiteId", memberUser.SourceSiteId);
            parameters.Add("ApproveOrderTimeInMilliseconds", totalRespondWaitingTimeInMilliseconds);
            parameters.Add("TakingOrderMerchantList", groupTakingOrderMerchantInfos);

            httpRequestMessage.SetAcceptJsonResponse();
            httpRequestMessage.SetJsonContent(parameters);

            return await SendRequestAsync<string>(httpRequestMessage, cancellationToken);
        }

        private async Task<NotifyApiResultBaseDto<ConfirmOrderResultDto>> CheckMakerRequestAnswer(int mmTakerSourceSiteId, string userSourceOrderId,
                                                                                                  CancellationToken cancellationToken)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, GetApiUrl(EnumOrderNotifyApi.CONFIRMORDERRESULT));

            var parameters = new Dictionary<string, object>();

            parameters.Add("SiteId", mmTakerSourceSiteId);
            parameters.Add("OrderId", userSourceOrderId);

            httpRequestMessage.SetAcceptJsonResponse();
            httpRequestMessage.SetJsonContent(parameters);

            return await SendRequestAsync<ConfirmOrderResultDto>(httpRequestMessage, cancellationToken);
        }

        private async Task<NotifyApiResultBaseDto<T>> SendRequestAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogTrace("Before HttpClient.SendAsync");

                var responseMessage = await HttpClient.SendAsync(httpRequestMessage, cancellationToken);

                _logger.LogTrace("After HttpClient.SendAsync");

                if (responseMessage.IsSuccessStatusCode == false)
                {
                    _logger.LogWarning("Post [{ApiRoute}] Get HttpStateCode:{StateCode},ReasonPhrase:{ReasonPhrase}",
                        httpRequestMessage.RequestUri?.PathAndQuery,
                        responseMessage.StatusCode,
                        responseMessage.ReasonPhrase);
                    return null;
                }

                _logger.LogTrace("Before Content.ReadAsStringAsync");

                var content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogTrace("After Content.ReadAsStringAsync");
                _logger.LogDebug("Post [{ApiRoute}] Response={Content}", httpRequestMessage.RequestUri?.PathAndQuery, content);

                var apiResult = content.JsonToObject<NotifyApiResultBaseDto<T>>();
                if (apiResult.Code != 200)
                {
                    _logger.LogWarning("Post [{ApiRoute}] Get RespondCode:{ApiResultCode},WithError={ErrorString}",
                        httpRequestMessage.RequestUri?.PathAndQuery,
                        apiResult.Code,
                        string.Join(",", apiResult.ExtraInfo.Select(y => $"{y.Field}={y.Value}")));

                    return null;
                }

                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }

        private static EnumMakerAnswer ParseEnumMakerAnswer(int resultOrderResult)
        {
            if (EnumMakerAnswer.Of(resultOrderResult) == EnumMakerAnswer.ACCEPT)
            {
                return EnumMakerAnswer.ACCEPT;
            }

            return EnumMakerAnswer.DENY;
        }

        private string GetApiUrl(EnumOrderNotifyApi takingorder)
        {
            var apiDomain = _configuration.GetValue<string>("NotifyApiDomain");
            return $"{apiDomain.TrimEnd('/')}/api/{takingorder.GetApiRoute()}";
        }
    }

    internal class GroupTakingOrderMerchantInfo
    {
        public int MmSiteId { get; set; }
        public List<MerchantInfoBase> MerchantInfoList { get; set; }
    }

    internal class MerchantInfoBase
    {
        public int MainOrderId { get; set; }
        public int MmUserId { get; set; }
    }
}