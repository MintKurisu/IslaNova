using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.PropertyManagement
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> builder)
        {
            #region Basic configuration
            builder.HasKey(p => p.PropertyId);
            builder.ToTable("Properties");
            #endregion

            #region Property configurations
            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(6);

            builder.HasIndex(p => p.Code)
                .IsUnique();

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.LandSize)
                .IsRequired();

            builder.Property(p => p.Bedrooms)
                .IsRequired();

            builder.Property(p => p.Bathrooms)
                .IsRequired();

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(p => p.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();
            #endregion

            #region Relationships
            builder.HasOne(p => p.PropertyType)
                .WithMany(pt => pt.Properties)
                .HasForeignKey(p => p.PropertyTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.SaleType)
                .WithMany(st => st.Properties)
                .HasForeignKey(p => p.SaleTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Images)
                .WithOne(pi => pi.Property)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PropertyImprovements)
                .WithOne(pi => pi.Property)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Favorites)
                .WithOne(f => f.Property)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Offers)
                .WithOne(o => o.Property)
                .HasForeignKey(o => o.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion
        }
    }
}
