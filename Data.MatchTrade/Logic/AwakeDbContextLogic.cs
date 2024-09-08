using Data.MatchTrade.Factories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.MatchTrade.Logic
{
    public class AwakeDbContextLogic : IAwakeDbContextLogic
    {
        private readonly IMatchTradeContextFactory _matchTradeContextFactory;

        public AwakeDbContextLogic(IMatchTradeContextFactory matchTradeContextFactory)
        {
            _matchTradeContextFactory = matchTradeContextFactory;
        }


        public void Awake()
        {
            var matchTradeContext = _matchTradeContextFactory.GetInstanceReadOnly;
            matchTradeContext.Database.ExecuteSqlRaw("select UTC_TIMESTAMP() as time");
        }
    }

    public interface IAwakeDbContextLogic
    {
        void Awake();
    }
}