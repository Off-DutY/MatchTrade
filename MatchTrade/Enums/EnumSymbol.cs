using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 交易對
    /// </summary>
    public class EnumSymbol : EnumClassBase
    {
        public static EnumSymbol BTC_USDT = new EnumSymbol(0, "BTC/USDT");
        
        private static readonly List<EnumSymbol> Values = new List<EnumSymbol>()
        {
            BTC_USDT
        };
        public static EnumSymbol Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        private EnumSymbol(int code, string desc) : base(code, desc)
        {
        }
    }
}