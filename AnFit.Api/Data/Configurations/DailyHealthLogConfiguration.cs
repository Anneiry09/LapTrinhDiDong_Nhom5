using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class DailyHealthLogConfiguration : IEntityTypeConfiguration<DailyHealthLog>
    {
        public void Configure(EntityTypeBuilder<DailyHealthLog> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__DailyHea__3214EC076A5B1858");

            builder.HasIndex(e => new { e.UserId, e.RecordDate }, "DailyHealthLogs_index_1").IsUnique();

            builder.HasIndex(e => new { e.UserId, e.ExternalSourceId }, "DailyHealthLogs_index_2");

            builder.Property(e => e.CalculatedTdee).HasColumnName("CalculatedTDEE");
            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.ExternalSourceId).HasMaxLength(255);
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.DailyHealthLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DailyHeal__UserI__5AEE82B9");
        }
    }
}
