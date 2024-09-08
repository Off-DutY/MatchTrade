using Data.MatchTrade.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.MatchTrade.Configurations
{
    public class MatchOrderMap : IEntityTypeConfiguration<OrderPool>
    {
        public void Configure(EntityTypeBuilder<OrderPool> builder)
        {
            builder.HasKey(r => r.Id);
            builder.ToTable(nameof(OrderPool));
        }
    }

    public class TradeResultMap : IEntityTypeConfiguration<TradeResult>
    {
        public void Configure(EntityTypeBuilder<TradeResult> builder)
        {
            builder.HasKey(r => r.Id);
            builder.ToTable(nameof(TradeResult));
        }
    }
}