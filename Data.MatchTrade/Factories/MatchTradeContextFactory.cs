using Data.MatchTrade.Factories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data.MatchTrade.Factories
{
    public class MatchTradeContextFactory : IMatchTradeContextFactory
    {
        private readonly object _lockObj = new object();
        private readonly IConfiguration _configuration;
        private MatchTradeContext _promotionContextInstance;
        private MatchTradeContext _promotionContextInstanceReadOnly;

        public MatchTradeContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MatchTradeContext GetInstance
        {
            get
            {
                if (_promotionContextInstance == null)
                {
                    lock (_lockObj)
                    {
                        _promotionContextInstance ??= GetNewInstance(false);
                    }
                }

                return _promotionContextInstance;
            }
        }

        public MatchTradeContext GetInstanceReadOnly
        {
            get
            {
                if (_promotionContextInstanceReadOnly == null)
                {
                    lock (_lockObj)
                    {
                        _promotionContextInstanceReadOnly ??= GetNewInstance(true);
                    }
                }

                return _promotionContextInstanceReadOnly;
            }
        }

        public MatchTradeContext GetNewInstance(bool isReadOnly)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<MatchTradeContext>();
            var contextConnString = GetConnectionString(isReadOnly);
            dbContextOptionsBuilder.UseMySql(contextConnString, ServerVersion.AutoDetect(contextConnString))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            return new MatchTradeContext(dbContextOptionsBuilder.Options);
        }

        private string GetConnectionString(bool isReadOnly = false)
        {
            var promotionContextName = nameof(MatchTradeContext);
            if (isReadOnly)
            {
                promotionContextName = promotionContextName.TrimEnd(';') + ";ReadOnly";
            }

            var contextConnString = _configuration.GetConnectionString(promotionContextName);
            return contextConnString;
        }
    }
}