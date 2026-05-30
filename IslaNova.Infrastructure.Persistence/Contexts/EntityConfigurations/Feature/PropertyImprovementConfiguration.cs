using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.Feature;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.Feature
{
    public class PropertyImprovementConfiguration : IEntityTypeConfiguration<PropertyImprovement>
    {
        public void Configure(EntityTypeBuilder<PropertyImprovement> builder)
        {
            #region Basic configuration
            builder.HasKey(pi => pi.PropertyImprovementId);
            builder.ToTable("PropertyImprovements");
            #endregion

            #region Property configurations
            builder.Property(pi => pi.PropertyId)
                .IsRequired();

            builder.Property(pi => pi.ImprovementId)
                .IsRequired();

            builder.HasIndex(pi => new { pi.PropertyId, pi.ImprovementId })
                .IsUnique();
            #endregion

            #region Relationships
            builder.HasOne(pi => pi.Property)
                .WithMany(p => p.PropertyImprovements)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pi => pi.Improvement)
                .WithMany(i => i.PropertyImprovements)
                .HasForeignKey(pi => pi.ImprovementId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
