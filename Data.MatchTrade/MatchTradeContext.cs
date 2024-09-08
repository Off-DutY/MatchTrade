using System.Reflection;
using Data.MatchTrade.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.MatchTrade
{
    public class MatchTradeContext : DbContext
    {
        public MatchTradeContext(DbContextOptions<MatchTradeContext> options) : base(options)
        {
        }

        public DbSet<OrderPool> OrderPools { get; set; }
        public DbSet<TradeResult> TradeResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            modelBuilder.ApplyConfigurationsFromAssembly(executingAssembly, type => type.Name.ToLower().EndsWith("map"));
        }
    }
}