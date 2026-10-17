using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace AnFit.Api.Data.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__Exercise__3214EC0702782631");

            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.InstructionUrl).HasMaxLength(500);
            builder.Property(e => e.MuscleGroup).HasMaxLength(100);
            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");


        }
    }
}
