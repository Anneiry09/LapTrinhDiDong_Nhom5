using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class UserBodyLogConfiguration : IEntityTypeConfiguration<UserBodyLog>
    {
        public void Configure(EntityTypeBuilder<UserBodyLog> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__UserBody__3214EC078F4B08DA");

            builder.Property(e => e.Bmi).HasColumnName("BMI");
            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.RecordedAt).HasColumnType("datetime");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.UserBodyLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserBodyL__UserI__571DF1D5");
        }
    }
}
