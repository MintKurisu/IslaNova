using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.Feature
{
    public class PropertyImprovementRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;

        public PropertyImprovementRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<(Property property, Improvement improvement)> SeedRequiredData(IslaNovaContext context)
        {
            var propertyType = new PropertyType { Name = "House", Description = "Test" };
            var saleType = new SaleType { Name = "Sale", Description = "Test" };
            var improvement = new Improvement { Name = "Pool", Description = "Swimming pool" };

            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Code = $"PROP{Guid.NewGuid().ToString().Substring(0, 6)}",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 250000m,
                LandSize = 500,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Test property",
                AgentId = "agent123",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            return (property, improvement);
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyImprovement_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);
            var repository = new PropertyImprovementRepository(context);

            var propertyImprovement = new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            };

            // Act
            var result = await repository.AddAsync(propertyImprovement);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyImprovementId.Should().BeGreaterThan(0);
            result.PropertyId.Should().Be(property.PropertyId);
            result.ImprovementId.Should().Be(improvement.ImprovementId);

            var propertyImprovementsInDb = await context.PropertyImprovements.ToListAsync();
            propertyImprovementsInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImprovementRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyImprovement_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);
            var repository = new PropertyImprovementRepository(context);

            var propertyImprovement = new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            };
            propertyImprovement = await repository.AddAsync(propertyImprovement);

            // Act
            var result = await repository.GetByIdAsync(propertyImprovement!.PropertyImprovementId);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyImprovementId.Should().Be(propertyImprovement.PropertyImprovementId);
            result.PropertyId.Should().Be(property.PropertyId);
            result.ImprovementId.Should().Be(improvement.ImprovementId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImprovementRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyImprovement()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);
            var repository = new PropertyImprovementRepository(context);

            var propertyImprovement = new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            };
            propertyImprovement = await repository.AddAsync(propertyImprovement);

            // Act
            await repository.DeleteAsync(propertyImprovement!.PropertyImprovementId);

            // Assert
            var entity = await repository.GetByIdAsync(propertyImprovement.PropertyImprovementId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImprovementRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_PropertyImprovements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);

            context.PropertyImprovements.AddRange(
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId },
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId },
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyImprovementRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_PropertyImprovements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImprovementRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_Should_Include_Property_And_Improvement()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);
            var repository = new PropertyImprovementRepository(context);

            var propertyImprovement = new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            };
            propertyImprovement = await repository.AddAsync(propertyImprovement);

            // Act
            var result = await repository.GetByIdWithIncludeAsync(
                propertyImprovement!.PropertyImprovementId,
                new List<string> { "Property", "Improvement" }
            );

            // Assert
            result.Should().NotBeNull();
            result!.Property.Should().NotBeNull();
            result.Property!.PropertyId.Should().Be(property.PropertyId);
            result.Improvement.Should().NotBeNull();
            result.Improvement!.ImprovementId.Should().Be(improvement.ImprovementId);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);

            context.PropertyImprovements.Add(new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            });
            await context.SaveChangesAsync();

            var repository = new PropertyImprovementRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_Property_And_Improvement()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);

            context.PropertyImprovements.Add(new PropertyImprovement
            {
                PropertyId = property.PropertyId,
                ImprovementId = improvement.ImprovementId
            });
            await context.SaveChangesAsync();

            var repository = new PropertyImprovementRepository(context);

            // Act
            var query = repository.GetAllQueryWithInclude(new List<string> { "Property", "Improvement" });
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Property.Should().NotBeNull();
            result[0].Improvement.Should().NotBeNull();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_PropertyImprovements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (property, improvement) = await SeedRequiredData(context);
            var repository = new PropertyImprovementRepository(context);

            var propertyImprovements = new List<PropertyImprovement>
            {
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId },
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId },
                new PropertyImprovement { PropertyId = property.PropertyId, ImprovementId = improvement.ImprovementId }
            };

            // Act
            var result = await repository.AddRangeAsync(propertyImprovements);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(pi => pi.PropertyImprovementId > 0);
        }
    }
}
