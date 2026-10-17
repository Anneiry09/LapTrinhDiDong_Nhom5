using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class MealPlanDetailConfiguration : IEntityTypeConfiguration<MealPlanDetail>
    {
        public void Configure(EntityTypeBuilder<MealPlanDetail> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__MealPlan__3214EC07530A096B");

            builder.Property(e => e.CalculatedPrice).HasColumnType("decimal(18, 2)");

            builder.HasOne(d => d.FoodItem).WithMany(p => p.MealPlanDetails)
                .HasForeignKey(d => d.FoodItemId)
                .HasConstraintName("FK__MealPlanD__FoodI__5FB337D6");

            builder.HasOne(d => d.Recipe).WithMany(p => p.MealPlanDetails)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(d => d.MealPlan).WithMany(p => p.MealPlanDetails)
                .HasForeignKey(d => d.MealPlanId)
                .HasConstraintName("FK__MealPlanD__MealP__5EBF139D");
        }
    }
}
