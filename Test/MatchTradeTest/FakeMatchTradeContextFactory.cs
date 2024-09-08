using Data.MatchTrade;
using Data.MatchTrade.Factories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MatchTradeTest
{
    public class FakeMatchTradeContextFactory : IMatchTradeContextFactory
    {
        private MatchTradeContext _getInstance;

        public MatchTradeContext GetInstance => _getInstance ??= GetNewInstance(false);

        public MatchTradeContext GetInstanceReadOnly => _getInstance ??= GetNewInstance(false);

        public MatchTradeContext GetNewInstance(bool isReadOnly)
        {
            var builder = new DbContextOptionsBuilder<MatchTradeContext>();
            builder.UseInMemoryDatabase("InMemoryMatchTradeDatabase");
            var context = new MatchTradeContext(builder.Options);
            return context;
        }
    }
}