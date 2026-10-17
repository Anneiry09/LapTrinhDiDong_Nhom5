using AnFit.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnFit.Api.Data.Configurations
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.EstimatedPrice).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.ImageUrl).HasMaxLength(500);
            builder.Property(e => e.CreatedAt).HasColumnType("datetime");
        }
    }
}
