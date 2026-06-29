using IslaNova.Core.Domain.Entities.AccountManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.AccountManagement
{
    public class AgentProfileConfiguration : IEntityTypeConfiguration<AgentProfile>
    {
        public void Configure(EntityTypeBuilder<AgentProfile> builder)
        {
            #region Basic configuration
            builder.HasKey(ap => ap.AgentProfileId);
            builder.ToTable("AgentProfiles");
            #endregion

            #region Property configurations
            builder.Property(ap => ap.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ap => ap.Bio)
                .HasMaxLength(1000);

            builder.Property(ap => ap.YearsOfExperience)
                .IsRequired(false);

            builder.Property(ap => ap.WhatsappNumber)
                .HasMaxLength(20);

            builder.Property(ap => ap.FacebookUrl)
                .HasMaxLength(300);

            builder.Property(ap => ap.InstagramUrl)
                .HasMaxLength(300);

            builder.Property(ap => ap.SpecialtyZones)
                .HasMaxLength(300);
            #endregion

            #region Indexes
            builder.HasIndex(ap => ap.AgentId)
                .IsUnique()
                .HasDatabaseName("IX_AgentProfiles_AgentId");
            #endregion
        }
    }
}
