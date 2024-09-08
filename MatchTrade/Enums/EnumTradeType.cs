using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public class EnumTradeType : EnumClassBase
    {
        public static EnumTradeType TAKER = new EnumTradeType(1, "吃单");
        public static EnumTradeType MAKER = new EnumTradeType(2, "挂单");
        public static EnumTradeType CANCEL = new EnumTradeType(3, "撤单");

        private static readonly List<EnumTradeType> Values = new List<EnumTradeType>()
        {
            TAKER,
            MAKER,
            CANCEL
        };

        public static EnumTradeType Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        protected EnumTradeType(int code, string desc) : base(code, desc)
        {
        }
    }
}