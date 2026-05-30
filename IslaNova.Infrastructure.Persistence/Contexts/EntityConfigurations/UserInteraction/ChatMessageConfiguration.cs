using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IslaNova.Core.Domain.Entities.UserInteraction;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.UserInteraction
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            #region Basic configuration
            builder.HasKey(cm => cm.ChatMessageId);
            builder.ToTable("ChatMessages");
            #endregion

            #region Property configurations
            builder.Property(cm => cm.PropertyId)
                .IsRequired();

            builder.Property(cm => cm.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cm => cm.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cm => cm.SenderId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cm => cm.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(cm => cm.SentAt)
                .IsRequired();

            builder.Property(cm => cm.IsRead)
                .IsRequired()
                .HasDefaultValue(false);
            #endregion

            #region Relationships
            builder.HasOne(cm => cm.Property)
                .WithMany(p => p.ChatMessages)
                .HasForeignKey(cm => cm.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
