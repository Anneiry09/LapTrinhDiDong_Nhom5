using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace AnFit.Api.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
         
                builder.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC0799C46DF8");

                builder.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F817438826CA").IsUnique();

                builder.Property(e => e.CreatedAt).HasColumnType("datetime");
                builder.Property(e => e.DeviceName).HasMaxLength(255);
                builder.Property(e => e.ExpiresAt).HasColumnType("datetime");
                builder.Property(e => e.IpAddress).HasMaxLength(45);
                builder.Property(e => e.ReplacedByToken).HasMaxLength(500);
                builder.Property(e => e.Token).HasMaxLength(500);

                builder.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__RefreshTo__UserI__5629CD9C");
          
        }
    }
}
