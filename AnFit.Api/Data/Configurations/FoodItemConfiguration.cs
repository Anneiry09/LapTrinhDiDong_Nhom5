using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace AnFit.Api.Data.Configurations
{
    public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
    {
        public void Configure(EntityTypeBuilder<FoodItem> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__FoodItem__3214EC0791A384E4");

            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.EstimatedPrice).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.Unit).HasMaxLength(50);
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            // B?ng trung gian FoodHealthWarnings
            builder.HasMany(d => d.Conditions).WithMany(p => p.FoodItems)
                .UsingEntity<Dictionary<string, object>>(
                    "FoodHealthWarning",
                    r => r.HasOne<MedicalCondition>().WithMany()
                        .HasForeignKey("ConditionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__FoodHealt__Condi__5CD6CB2B"),
                    l => l.HasOne<FoodItem>().WithMany()
                        .HasForeignKey("FoodItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__FoodHealt__FoodI__5BE2A6F2"),
                    j =>
                    {
                        j.HasKey("FoodItemId", "ConditionId").HasName("PK__FoodHeal__A532941EAE83FE7B");
                        j.ToTable("FoodHealthWarnings");
                    });
        }
    }
}
