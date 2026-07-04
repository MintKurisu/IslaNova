using IslaNova.Core.Domain.Entities.AccountManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.AccountManagement
{
    public class AgentApplicationConfiguration : IEntityTypeConfiguration<AgentApplication>
    {
        public void Configure(EntityTypeBuilder<AgentApplication> builder)
        {
            #region Basic configuration

            builder.HasKey(a => a.ApplicationId);
            builder.ToTable("AgentApplications");

            #endregion

            #region Property configurations

            builder.Property(a => a.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.LicenseNumber)
                .HasMaxLength(100);

            builder.Property(a => a.CertificationNumber)
                .HasMaxLength(100);

            builder.Property(a => a.AgencyName)
                .HasMaxLength(200);

            builder.Property(a => a.ProfessionalStatement)
                .IsRequired()
                .HasMaxLength(1500);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.ReviewedBy)
                .HasMaxLength(450);

            builder.Property(a => a.AdminComments)
                .HasMaxLength(1000);

            #endregion

            #region Indexes

            builder.HasIndex(a => a.UserId)
                .IsUnique()
                .HasDatabaseName("IX_AgentApplications_UserId");

            builder.HasIndex(a => a.Status);

            #endregion
        }
    }
}
