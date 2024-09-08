using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    public class EnumMakerAnswer : EnumClassBase
    {
        public static EnumMakerAnswer WAITING = new EnumMakerAnswer(0, "接单中");
        public static EnumMakerAnswer ACCEPT = new EnumMakerAnswer(1, "已接单");
        public static EnumMakerAnswer DENY = new EnumMakerAnswer(2, "订单拒绝");
        public static EnumMakerAnswer TIMEOUT = new EnumMakerAnswer(3, "订单过期");

        private static readonly List<EnumMakerAnswer> Values = new List<EnumMakerAnswer>()
        {
            WAITING,
            ACCEPT,
            DENY,
            TIMEOUT
        };

        public static EnumMakerAnswer Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        private EnumMakerAnswer(int code, string desc) : base(code, desc)
        {
        }
    }
}