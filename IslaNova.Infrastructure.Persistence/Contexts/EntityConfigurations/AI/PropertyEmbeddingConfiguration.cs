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

            // Map float[] to pgvector column (1536 dimensions for OpenAI text-embedding-3-small)
            builder.Property(e => e.Embedding)
                .HasColumnType("vector(1536)");

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
        }
    }
}
