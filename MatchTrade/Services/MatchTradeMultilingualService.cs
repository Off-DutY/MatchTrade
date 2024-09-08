using CoreLibrary.Interface;

namespace MatchTrade.Services
{
    public sealed class MatchTradeMultilingualService : IMultilingualService
    {
        public string Get(string textKey, string defaultText = null)
        {
            return defaultText ?? textKey;
        }
    }
}