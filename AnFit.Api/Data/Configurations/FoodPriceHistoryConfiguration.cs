using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class FoodPriceHistoryConfiguration : IEntityTypeConfiguration<FoodPriceHistory>
    {
        public void Configure(EntityTypeBuilder<FoodPriceHistory> builder)
        {
            builder.ToTable("FoodPriceHistory");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.RecordedAt).HasColumnType("datetime2");
            builder.Property(e => e.Source).HasMaxLength(100);

            builder.HasOne(d => d.FoodItem)
                .WithMany(p => p.PriceHistory)
                .HasForeignKey(d => d.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index: tìm giá mới nhất theo FoodItem nhanh
            builder.HasIndex(e => new { e.FoodItemId, e.RecordedAt })
                .IsDescending(false, true)
                .HasDatabaseName("IX_FoodPriceHistory_FoodItem_Date");
        }
    }
}
