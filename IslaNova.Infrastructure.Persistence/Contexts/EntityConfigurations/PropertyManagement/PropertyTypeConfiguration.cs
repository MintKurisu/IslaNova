using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.PropertyManagement
{
    public class PropertyTypeConfiguration : IEntityTypeConfiguration<PropertyType>
    {
        public void Configure(EntityTypeBuilder<PropertyType> builder)
        {
            #region Basic configuration
            builder.HasKey(pt => pt.PropertyTypeId);
            builder.ToTable("PropertyTypes");
            #endregion

            #region Property configurations
            builder.Property(pt => pt.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pt => pt.Description)
                .IsRequired()
                .HasMaxLength(500);
            #endregion

            #region Relationships
            builder.HasMany(pt => pt.Properties)
                .WithOne(p => p.PropertyType)
                .HasForeignKey(p => p.PropertyTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
