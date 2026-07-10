using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Entities.AI;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Entities.UserInteraction;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IslaNova.Infrastructure.Persistence.Contexts
{
    public class IslaNovaContext : DbContext
    {
        public IslaNovaContext(DbContextOptions<IslaNovaContext> options) : base(options) { }

        // Feature
        public DbSet<Improvement> Improvements { get; set; }
        public DbSet<PropertyImprovement> PropertyImprovements { get; set; }

        // Property
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<SaleType> SaleTypes { get; set; }

        // UserInteraction
        public DbSet<Offer> Offers { get; set; }

        // AccountManagement
        public DbSet<AgentProfile> AgentProfiles { get; set; }
        public DbSet<AgentApplication> AgentApplications { get; set; }

        // AI — Vector embeddings for RAG chatbot
        public DbSet<PropertyEmbedding> PropertyEmbeddings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enable pgvector extension (required for Supabase vector columns)
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
