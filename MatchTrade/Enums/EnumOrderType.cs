using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 訂單類型
    /// </summary>
    public class EnumOrderType : EnumClassBase
    {
        /// <summary>
        /// 正常比價，
        /// </summary>
        public static EnumOrderType Member = new EnumOrderType(1, "会员挂单");
        public static EnumOrderType MMPay = new EnumOrderType(2, "承兑商挂单");

        private static readonly List<EnumOrderType> Values = new List<EnumOrderType>()
        {
            Member,
            MMPay
        };

        public static EnumOrderType Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        // static EnumOrderType()
        // {
        //     Values.Add(Member);
        //     Values.Add(MMPay);
        // }

        private EnumOrderType(int code, string desc)
                : base(code, desc)
        {
        }
    }
}