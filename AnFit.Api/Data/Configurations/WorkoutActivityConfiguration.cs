using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class WorkoutActivityConfiguration : IEntityTypeConfiguration<WorkoutActivity>
    {
        public void Configure(EntityTypeBuilder<WorkoutActivity> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__WorkoutA__3214EC07FF6195DD");

            builder.HasOne(d => d.Exercise).WithMany(p => p.WorkoutActivities)
                .HasForeignKey(d => d.ExerciseId)
                .HasConstraintName("FK__WorkoutAc__Exerc__628FA481");

            builder.HasOne(d => d.WorkoutPlan).WithMany(p => p.WorkoutActivities)
                .HasForeignKey(d => d.WorkoutPlanId)
                .HasConstraintName("FK__WorkoutAc__Worko__619B8048");
        }
    }
}
