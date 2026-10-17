using AnFit.Api.Models.Entities; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace AnFit.Api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__Users__3214EC07AB13AA58");

            builder.HasIndex(e => e.Email, "UQ__Users__A9D10534CFAAD8AB").IsUnique();

            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.DailyBudget).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.DateOfBirth).HasColumnType("datetime");
            builder.Property(e => e.DeletedAt).HasColumnType("datetime");
            builder.Property(e => e.Email).HasMaxLength(255);
            builder.Property(e => e.FullName).HasMaxLength(255);
            builder.Property(e => e.PasswordHash).HasMaxLength(500);
            builder.Property(e => e.PasswordResetToken).HasMaxLength(500);
            builder.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            builder.Property(e => e.Role).HasDefaultValue(1);
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");
            builder.Property(e => e.IsCompensateExercise).HasDefaultValue(true);

            // X? l� b?ng trung gian Many-to-Many
            builder.HasMany(d => d.Conditions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserMedicalCondition",
                    r => r.HasOne<MedicalCondition>().WithMany()
                        .HasForeignKey("ConditionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserMedic__Condi__59063A47"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserMedic__UserI__5812160E"),
                    j =>
                    {
                        j.HasKey("UserId", "ConditionId").HasName("PK__UserMedi__F4F790408B9798AE");
                        j.ToTable("UserMedicalConditions");
                    });
        }
    }
}
