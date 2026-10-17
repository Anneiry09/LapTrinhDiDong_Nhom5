using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class DailyProgressRecordConfiguration : IEntityTypeConfiguration<DailyProgressRecord>
    {
        public void Configure(EntityTypeBuilder<DailyProgressRecord> builder)
        {
            builder.HasKey(e => e.Id);

            // Chỉ cho phép tối đa 1 bản ghi snapshot mỗi ngày cho mỗi User
            builder.HasIndex(e => new { e.UserId, e.RecordDate }, "DailyProgressRecords_index_1").IsUnique();

            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");
            builder.Property(e => e.WorkoutPlanName).HasMaxLength(255);

            builder.HasOne(d => d.User).WithMany(p => p.DailyProgressRecords)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_DailyProgressRecords_Users_UserId");
        }
    }
}
