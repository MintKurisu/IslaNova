using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.PropertyManagement
{
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(EntityTypeBuilder<PropertyImage> builder)
        {
            #region Basic configuration
            builder.HasKey(pi => pi.PropertyImageId);
            builder.ToTable("PropertyImages");
            #endregion

            #region Property configurations
            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pi => pi.PropertyId)
                .IsRequired();
            #endregion

            #region Relationships
            builder.HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
