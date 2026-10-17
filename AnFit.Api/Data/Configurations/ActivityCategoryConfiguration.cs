using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace AnFit.Api.Data.Configurations
{
    public class ActivityCategoryConfiguration : IEntityTypeConfiguration<ActivityCategory>
    {
        public void Configure(EntityTypeBuilder<ActivityCategory> builder)
        {
            builder.ToTable("ActivityCategories");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Many-to-Many: User <-> ActivityCategory via UserPreferredActivities
            builder.HasMany(d => d.Users).WithMany(p => p.PreferredActivities)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPreferredActivity",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<ActivityCategory>().WithMany()
                        .HasForeignKey("ActivityCategoryId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("UserId", "ActivityCategoryId");
                        j.ToTable("UserPreferredActivities");
                    });
        }
    }
}
