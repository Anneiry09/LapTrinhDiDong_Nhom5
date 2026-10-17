using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
    {
        public void Configure(EntityTypeBuilder<MedicalCondition> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__MedicalC__3214EC07F15B4426");

            builder.Property(e => e.ConditionName).HasMaxLength(255);
        }
    }
}
