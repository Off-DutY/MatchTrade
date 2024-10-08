﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace MatchTrade.Middlewares
{
    public class RecordRequestInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RecordRequestInfoMiddleware(RequestDelegate next, ILogger<RecordRequestInfoMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/Home/Awake")
            {
                await _next(context);
            }
            else
            {
                var randomNum = DateTime.UtcNow.Ticks;
                await LogRequest(context, randomNum);
                await LogResponse(context, randomNum);
            }
        }

        private async Task LogRequest(HttpContext context, long randomNum)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            _logger.LogInformation(@"[{RandomNum}]Http Request Information:
Schema:{Scheme}  Host: {Host}  Path: {Path}  QueryString: {QueryString} 
Request Body: {RequestBody}",
                randomNum, context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, ReadStreamInChunks(requestStream));
            context.Request.Body.Position = 0;
        }

        private async Task LogResponse(HttpContext context, long randomNum)
        {
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;
            // Go Next
            await _next(context);
            // Finish
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            _logger.LogInformation(
                @"[{RandomNum}]Http Response Information:
Schema:{Scheme}  Host: {Host}  Path: {Path}  QueryString: {QueryString} 
Response Body: {ResponseBody}", randomNum, context.Request.Scheme, context.Request.Host, context.Request.Path, context.Request.QueryString, text);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                    0,
                    readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }

    public static class RecordRequestInfoMiddlewareExtensions
    {
        public static IApplicationBuilder UseRecordRequestInfoMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RecordRequestInfoMiddleware>();
        }
    }
}