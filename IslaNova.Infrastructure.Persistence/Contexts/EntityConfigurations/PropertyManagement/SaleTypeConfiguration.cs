using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.PropertyManagement;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.PropertyManagement
{
    public class SaleTypeConfiguration : IEntityTypeConfiguration<SaleType>
    {
        public void Configure(EntityTypeBuilder<SaleType> builder)
        {
            #region Basic configuration
            builder.HasKey(st => st.SaleTypeId);
            builder.ToTable("SaleTypes");
            #endregion

            #region Property configurations
            builder.Property(st => st.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(st => st.Description)
                .IsRequired()
                .HasMaxLength(500);
            #endregion

            #region Relationships
            builder.HasMany(st => st.Properties)
                .WithOne(p => p.SaleType)
                .HasForeignKey(p => p.SaleTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
