using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.UserInteraction
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            #region Basic configuration
            builder.HasKey(f => f.FavoriteId);
            builder.ToTable("Favorites");
            #endregion

            #region Property configurations
            builder.Property(f => f.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(f => f.PropertyId)
                .IsRequired();

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.HasIndex(f => new { f.ClientId, f.PropertyId })
                .IsUnique();
            #endregion

            #region Relationships
            builder.HasOne(f => f.Property)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
