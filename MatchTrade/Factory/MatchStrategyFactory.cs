using MatchTrade.Enums;
using MatchTrade.Services;
using MatchTrade.Services.Interface;

namespace MatchTrade.Factory
{
    public class MatchStrategyFactory
    {
        private readonly MmPartnerOrderMatchService _mmPartnerOrderMatchService;
        private readonly MemberOrderMatchService _memberOrderMatchService;

        public MatchStrategyFactory(MmPartnerOrderMatchService mmPartnerOrderMatchService, MemberOrderMatchService memberOrderMatchService)
        {
            _mmPartnerOrderMatchService = mmPartnerOrderMatchService;
            _memberOrderMatchService = memberOrderMatchService;
        }

        public IOrderMatchService GetByOrderType(EnumOrderType enumOrderType)
        {
            if (enumOrderType == EnumOrderType.MMPay)
            {
                return _mmPartnerOrderMatchService;
            }

            return _memberOrderMatchService;
        }
    }
}