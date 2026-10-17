using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class UserIntegrationConfiguration : IEntityTypeConfiguration<UserIntegration>
    {
        public void Configure(EntityTypeBuilder<UserIntegration> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK__UserInte__3214EC077CFFEC7E");

            builder.HasIndex(e => new { e.UserId, e.Provider }, "UserIntegrations_index_0").IsUnique();

            builder.Property(e => e.AccessToken).HasColumnType("text");
            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
            builder.Property(e => e.ExternalAccountId).HasMaxLength(255);
            builder.Property(e => e.RefreshToken).HasColumnType("text");
            builder.Property(e => e.TokenExpiry).HasColumnType("datetime");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetime");

            builder.HasOne(d => d.User).WithMany(p => p.UserIntegrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserInteg__UserI__59FA5E80");
        }
    }
}
