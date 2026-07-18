using IslaNova.Core.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IslaNova.Infrastructure.Persistence.Contexts.EntityConfigurations.AI
{
    public class PropertyEmbeddingConfiguration : IEntityTypeConfiguration<PropertyEmbedding>
    {
        public void Configure(EntityTypeBuilder<PropertyEmbedding> builder)
        {
            builder.ToTable("PropertyEmbeddings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.PropertyId)
                .IsRequired();

            builder.HasIndex(e => e.PropertyId)
                .IsUnique()
                .HasDatabaseName("IX_PropertyEmbeddings_PropertyId");

            builder.Property(e => e.PlainText)
                .IsRequired()
                .HasColumnType("text");

            // Embedding is marked [NotMapped] in the entity — EF will not try to map it.
            // The vector(1536) column is added manually in the migration and managed via raw SQL.
            builder.Ignore(e => e.Embedding);

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        }
    }
}
