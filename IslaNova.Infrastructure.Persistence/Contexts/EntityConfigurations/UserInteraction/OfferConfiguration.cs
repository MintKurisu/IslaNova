using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.UserInteraction
{
    public class OfferConfiguration : IEntityTypeConfiguration<Offer>
    {
        public void Configure(EntityTypeBuilder<Offer> builder)
        {
            #region Basic configuration
            builder.HasKey(o => o.OfferId);
            builder.ToTable("Offers");
            #endregion

            #region Property configurations
            builder.Property(o => o.PropertyId)
                .IsRequired();

            builder.Property(o => o.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Status)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            #endregion

            #region Relationships
            builder.HasOne(o => o.Property)
                .WithMany(p => p.Offers)
                .HasForeignKey(o => o.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
