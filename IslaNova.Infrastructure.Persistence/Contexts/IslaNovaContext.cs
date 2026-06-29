using IslaNova.Core.Domain.Entities.AccountManagement;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
