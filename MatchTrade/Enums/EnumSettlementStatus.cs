using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 清结算状态
    /// </summary>
    public class EnumSettlementStatus : EnumClassBase
    {
        public static EnumSettlementStatus UN_DO = new EnumSettlementStatus(0, "未清算");
        public static EnumSettlementStatus DONE = new EnumSettlementStatus(1, "已清算");

        private static readonly List<EnumSettlementStatus> Values = new List<EnumSettlementStatus>()
        {
            UN_DO,
            DONE
        };
        public static EnumSettlementStatus Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }
        
        private EnumSettlementStatus(int code, string desc) : base(code, desc)
        {
        }
    }
}