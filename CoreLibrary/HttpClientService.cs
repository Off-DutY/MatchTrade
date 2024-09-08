namespace CoreLibrary
{
    public sealed class HttpClientService
    {
        private static Lazy<HttpClient> lazy = new Lazy<HttpClient>(() => new HttpClient());

        private HttpClientService()
        {
        }

        public static HttpClient Instance { get { return lazy.Value; } }
    }
}