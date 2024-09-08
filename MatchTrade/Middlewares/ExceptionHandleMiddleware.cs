using System;
using System.Net;
using System.Threading.Tasks;
using CoreLibrary.Dto.BaseResponses;
using CoreLibrary.Interface;
using GameSupplierApi.Common.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MatchTrade.Middlewares
{
    public class ExceptionHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IApiResultService _apiResultService;

        public ExceptionHandleMiddleware(RequestDelegate next, ILogger<ExceptionHandleMiddleware> logger,
                                         IApiResultService apiResultService)
        {
            _next = next;
            _logger = logger;
            _apiResultService = apiResultService;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, exception.Message);
                await HandleExceptionAsync(context, exception);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status200OK;

            ApiResultDto<object> resultDto;

            switch (exception)
            {
                default:
                    resultDto = _apiResultService.Error<object>(new Exception(HttpStatusCode.BadRequest.ToString()));
                    resultDto.AddExtraInfo("Error", exception.Message);
                    break;
            }

            return context.Response.WriteAsync(resultDto.ToJson());
        }
    }

    public static class ExceptionHandleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandleMiddleware>();
        }
    }
}