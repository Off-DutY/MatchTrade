using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 支付状态
    /// </summary>
    public class EnumPayStatus : EnumClassBase
    {
        public static EnumPayStatus UNPAID = new EnumPayStatus(0, "未支付");
        public static EnumPayStatus PAID = new EnumPayStatus(1, "已支付 ");
        public static EnumPayStatus ERROR = new EnumPayStatus(2, "异常订单");
        public static EnumPayStatus FAIL = new EnumPayStatus(3, "支付失败");

        private static readonly List<EnumPayStatus> Values = new List<EnumPayStatus>()
        {
            UNPAID,
            PAID,
            ERROR,
            FAIL
        };

        public static EnumPayStatus Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        private EnumPayStatus(int code, string desc) : base(code, desc)
        {
        }
    }
}