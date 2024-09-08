using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 訂單動作
    /// </summary>
    public class EnumOrderAction : EnumClassBase
    {
        public static EnumOrderAction ASK = new EnumOrderAction(0);
        public static EnumOrderAction BUY = new EnumOrderAction(1);

        private static readonly List<EnumOrderAction> Values = new List<EnumOrderAction>()
        {
            ASK,
            BUY
        };
        public static EnumOrderAction Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }
        
        private EnumOrderAction(int code) :
                base(code, "")
        {
        }
    }
}