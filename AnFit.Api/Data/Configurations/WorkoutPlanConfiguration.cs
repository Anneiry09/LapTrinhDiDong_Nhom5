using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__WorkoutP__3214EC07ED540981");

            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.WorkoutPlans)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__WorkoutPl__UserI__60A75C0F");
        }
    }
}
