using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.Feature;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.Feature
{
    public class ImprovementConfiguration : IEntityTypeConfiguration<Improvement>
    {
        public void Configure(EntityTypeBuilder<Improvement> builder)
        {
            #region Basic configuration
            builder.HasKey(i => i.ImprovementId);
            builder.ToTable("Improvements");
            #endregion

            #region Property configurations
            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(500);
            #endregion

            #region Relationships
            builder.HasMany(i => i.PropertyImprovements)
                .WithOne(pi => pi.Improvement)
                .HasForeignKey(pi => pi.ImprovementId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
