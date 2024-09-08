using System.Collections.Generic;
using System.Linq;

namespace MatchTrade.Enums
{
    public class EnumOrderNotifyApi : EnumClassBase
    {
        private readonly string _apiRoute;
        public static EnumOrderNotifyApi TAKINGORDER = new EnumOrderNotifyApi(1, "接单", "MmTrade/TakingOrder");
        public static EnumOrderNotifyApi CONFIRMORDERRESULT = new EnumOrderNotifyApi(2, "确认接单状态", "MmTrade/ConfirmOrderResult");

        private static List<EnumOrderNotifyApi> Values = new List<EnumOrderNotifyApi>()
        {
            TAKINGORDER,
            CONFIRMORDERRESULT,
        };

        public static EnumOrderNotifyApi Of(int code)
        {
            return Values.FirstOrDefault(r => r.Code == code);
        }

        public EnumOrderNotifyApi(int code, string desc, string apiRoute) : base(code, desc)
        {
            _apiRoute = apiRoute;
        }

        public string GetApiRoute()
        {
            return _apiRoute;
        }
    }
}