using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
    {
        public void Configure(EntityTypeBuilder<MealPlan> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__MealPlan__3214EC07D50D9151");

            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.MealPlans)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__MealPlans__UserI__5DCAEF64");
        }
    }
}
