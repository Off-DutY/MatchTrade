using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    /// <summary>
    /// 訂單狀態
    /// </summary>
    public class EnumOrderState : EnumClassBase
    {
        public static EnumOrderState ERROR = new EnumOrderState(-1, "委托异常");
        public static EnumOrderState REQUEST = new EnumOrderState(0, "委托请求");
        public static EnumOrderState ORDER = new EnumOrderState(1, "委托中");
        public static EnumOrderState SOME_DEAL = new EnumOrderState(2, "部份成交 ");
        public static EnumOrderState ALL_DEAL = new EnumOrderState(3, "全部成交");
        public static EnumOrderState CANCEL = new EnumOrderState(4, "已撤销");

        private static readonly List<EnumOrderState> Values = new List<EnumOrderState>()
        {
            ERROR,
            REQUEST,
            ORDER,
            SOME_DEAL,
            ALL_DEAL,
            CANCEL,
        };

        public static EnumOrderState Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        private EnumOrderState(int code, string desc)
                : base(code, desc)
        {
        }
    }
}