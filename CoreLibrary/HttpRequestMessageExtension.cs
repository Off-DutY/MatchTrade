using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Web;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreLibrary
{
    public static class HttpRequestMessageExtension
    {
        private static readonly int DnsRefreshTimeout = (int)TimeSpan.FromMinutes(5.0).TotalMilliseconds;
        private static readonly string TimeoutPropertyKey = "RequestTimeout";
        private static readonly Serilog.ILogger Logger = Serilog.Log.Logger;

        static HttpRequestMessageExtension()
        {
            ServicePointManager.DnsRefreshTimeout = DnsRefreshTimeout;
            ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)((sender, cert, chain, sslPolicyErrors) => true);
        }


        public static async Task<HttpResponseMessage> ReceiveAsync(this HttpRequestMessage reqMsg, ILogger logger)
        {
            if (reqMsg == null)
                throw new ArgumentNullException(nameof(reqMsg));
            var logPrefix = string.Format("({0}Async:{1})", reqMsg.Method, DateTime.UtcNow.Ticks);

            if (reqMsg.Content != null)
            {
                var logString = await reqMsg.Content.ReadAsStringAsync().ConfigureAwait(false);
                logger.LogInformation(string.Format("{0}Content={1}", logPrefix, logString));
            }

            logger.LogInformation(string.Format("{0}({1})", logPrefix, reqMsg.RequestUri));
            return await HttpClientService.Instance.SendAsync(reqMsg).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> ReceiveAsync(this HttpRequestMessage reqMsg)
        {
            if (reqMsg == null)
                throw new ArgumentNullException(nameof(reqMsg));
            var logPrefix = string.Format("({0}Async:{1})", reqMsg.Method, DateTime.UtcNow.Ticks);

            if (reqMsg.Content != null)
            {
                var logString = await reqMsg.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.Debug(string.Format("{0}Content={1}", logPrefix, logString));
            }

            Logger.Debug(string.Format("{0}({1})", logPrefix, reqMsg.RequestUri));
            return await HttpClientService.Instance.SendAsync(reqMsg).ConfigureAwait(false);
        }

        public static async Task<string> ReceiveAsStringAsync(this HttpRequestMessage reqMsg, Encoding encoding = null)
        {
            if (reqMsg == null)
                throw new ArgumentNullException(nameof(reqMsg));
            var logPrefix = string.Format("({0}Async:{1})", reqMsg.Method, DateTime.UtcNow.Ticks);
            if (encoding == null)
                encoding = Encoding.UTF8;
            if (reqMsg.Content != null)
            {
                var logString = await reqMsg.Content.ReadAsStringAsync().ConfigureAwait(false);
                Logger.Debug(string.Format("{0}Content={1}", logPrefix, logString));
            }

            Logger.Debug(string.Format("{0}({1})", logPrefix, reqMsg.RequestUri));
            var response = await HttpClientService.Instance.SendAsync(reqMsg).ConfigureAwait(false);
            var asStringAsync = encoding.GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
            Logger.Debug(string.Format("{0}call api got HTTP Status={1}, result=\"{2}\"", logPrefix, (int)response.StatusCode, asStringAsync));
            return asStringAsync;
        }

        public static async Task<T> ReceiveAsObjectAsync<T>(this HttpRequestMessage reqMsg, Encoding encoding = null)
        {
            var asStringAsync = await ReceiveAsStringAsync(reqMsg, encoding).ConfigureAwait(false);
            var deserializeObject = JsonConvert.DeserializeObject<T>(asStringAsync);
            return deserializeObject;
        }

        public static void SetBearerAuthorization(this HttpRequestMessage request, string authToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(authToken))
                return;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public static void SetJsonContent(this HttpRequestMessage request, Dictionary<string, object> data, Encoding encoding = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (data == null)
                return;
            if (encoding == null)
                encoding = Encoding.UTF8;
            request.SetJsonContent(JsonConvert.SerializeObject(data), encoding);
        }

        public static void SetJsonContent(this HttpRequestMessage request, Dictionary<string, string> data, Encoding encoding = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (data == null)
                return;
            if (encoding == null)
                encoding = Encoding.UTF8;
            request.SetJsonContent(JsonConvert.SerializeObject(data), encoding);
        }

        public static void SetJsonContent(this HttpRequestMessage request, string data, Encoding encoding = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(data))
                return;
            if (encoding == null)
                encoding = Encoding.UTF8;
            request.Content = new StringContent(data, encoding, "application/json");
        }

        public static void SetStringContent(this HttpRequestMessage request, string data, Encoding encoding = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(data))
                return;
            if (encoding == null)
                encoding = Encoding.UTF8;
            request.Content = new StringContent(data, encoding);
        }

        public static void SetQueryString(this HttpRequestMessage request, Dictionary<string, string> data)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (data == null)
                return;
            request.RequestUri = AppendParameters(request.RequestUri, data);
        }

        public static void SetFormContent(this HttpRequestMessage request, Dictionary<string, string> data)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (data == null)
                return;
            request.Content = new FormUrlEncodedContent(data);
        }

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (!timeout.HasValue)
                return;
            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static void SetTimeout(this HttpRequestMessage request, double? timeoutSecond)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (!timeoutSecond.HasValue || timeoutSecond.Value <= 0.0)
                return;
            request.SetTimeout(TimeSpan.FromSeconds(timeoutSecond.Value));
        }

        public static void SetAcceptFormResponse(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        }

        public static void SetAcceptJsonResponse(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static Uri AppendParameters(this Uri uri, Dictionary<string, string> parameters)
        {
            var uriBuilder = new UriBuilder(uri);
            if (parameters != null)
            {
                var queryString = HttpUtility.ParseQueryString(uriBuilder.Query ?? string.Empty);
                foreach (var parameter in parameters)
                    queryString.Add(parameter.Key, parameter.Value);
                uriBuilder.Query = queryString.ToString();
            }

            return uriBuilder.Uri;
        }
    }
}