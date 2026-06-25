using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.PropertyManagement
{
    public class PropertyRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public PropertyRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<(PropertyType propertyType, SaleType saleType)> SeedRequiredData(IslaNovaContext context)
        {
            var propertyType = new PropertyType { Name = "House", Description = "Family house" };
            var saleType = new SaleType { Name = "Sale", Description = "For sale" };

            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            return (propertyType, saleType);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Property_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var property = new Property
            {
                Code = "PROP001",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 250000m,
                LandSize = 500,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Beautiful house",
                AgentId = "agent123",
                Status = PropertyStatus.Available
            };

            // Act
            var result = await repository.AddAsync(property);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyId.Should().BeGreaterThan(0);
            result.Code.Should().Be("PROP001");
            result.Status.Should().Be(PropertyStatus.Available);

            var propertiesInDb = await context.Properties.ToListAsync();
            propertiesInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Property_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var property = new Property
            {
                Code = "PROP002",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 180000m,
                LandSize = 300,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Cozy apartment",
                AgentId = "agent456",
                Status = PropertyStatus.Available
            };
            property = await repository.AddAsync(property);

            // Act
            var result = await repository.GetByIdAsync(property!.PropertyId);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyId.Should().Be(property.PropertyId);
            result.Code.Should().Be("PROP002");
            result.Price.Should().Be(180000m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Property_In_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var property = new Property
            {
                Code = "PROP003",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 200000m,
                LandSize = 400,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Nice house",
                AgentId = "agent789",
                Status = PropertyStatus.Available
            };
            property = await repository.AddAsync(property);
            property!.Price = 210000m;
            property.Description = "Updated nice house";

            // Act
            var updated = await repository.UpdateAsync(property.PropertyId, property);

            // Assert
            updated.Should().NotBeNull();
            updated!.Price.Should().Be(210000m);
            updated.Description.Should().Be("Updated nice house");

            var fromDb = await repository.GetByIdAsync(property.PropertyId);
            fromDb!.Price.Should().Be(210000m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Property_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var fakeProperty = new Property
            {
                PropertyId = 9999,
                Code = "FAKE",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 100000m,
                LandSize = 100,
                Bedrooms = 1,
                Bathrooms = 1,
                Description = "Fake",
                AgentId = "agent999",
                Status = PropertyStatus.Available
            };

            // Act
            var updated = await repository.UpdateAsync(9999, fakeProperty);

            // Assert
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var property = new Property
            {
                Code = "PROP004",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 150000m,
                LandSize = 250,
                Bedrooms = 2,
                Bathrooms = 1,
                Description = "Small house",
                AgentId = "agent111",
                Status = PropertyStatus.Available
            };
            property = await repository.AddAsync(property);

            // Act
            await repository.DeleteAsync(property!.PropertyId);

            // Assert
            var entity = await repository.GetByIdAsync(property.PropertyId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Properties()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);

            context.Properties.AddRange(
                new Property
                {
                    Code = "P1",
                    PropertyTypeId = propertyType.PropertyTypeId,
                    SaleTypeId = saleType.SaleTypeId,
                    Price = 100000m,
                    LandSize = 100,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Description = "Prop 1",
                    AgentId = "a1",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Code = "P2",
                    PropertyTypeId = propertyType.PropertyTypeId,
                    SaleTypeId = saleType.SaleTypeId,
                    Price = 200000m,
                    LandSize = 200,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Prop 2",
                    AgentId = "a2",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Code = "P3",
                    PropertyTypeId = propertyType.PropertyTypeId,
                    SaleTypeId = saleType.SaleTypeId,
                    Price = 300000m,
                    LandSize = 300,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Prop 3",
                    AgentId = "a3",
                    Status = PropertyStatus.Available
                }
            );

            await context.SaveChangesAsync();
            var repository = new PropertyRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }


        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_Properties()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_Should_Return_Property_With_Relations()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);
            var repository = new PropertyRepository(context);

            var property = new Property
            {
                Code = "PROP005",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 300000m,
                LandSize = 600,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Luxury property",
                AgentId = "agent999",
                Status = PropertyStatus.Available
            };
            property = await repository.AddAsync(property);

            // Act
            var result = await repository.GetByIdWithIncludeAsync(
                property!.PropertyId,
                new List<string> { "PropertyType", "SaleType" }
            );

            // Assert
            result.Should().NotBeNull();
            result!.PropertyType.Should().NotBeNull();
            result.PropertyType!.Name.Should().Be("House");
            result.SaleType.Should().NotBeNull();
            result.SaleType!.Name.Should().Be("Sale");
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var (propertyType, saleType) = await SeedRequiredData(context);

            context.Properties.AddRange(
                new Property
                {
                    Code = "AVAILABLE1",
                    PropertyTypeId = propertyType.PropertyTypeId,
                    SaleTypeId = saleType.SaleTypeId,
                    Price = 100000m,
                    LandSize = 100,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    Description = "Prop 1",
                    AgentId = "a1",
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Code = "SOLD1",
                    PropertyTypeId = propertyType.PropertyTypeId,
                    SaleTypeId = saleType.SaleTypeId,
                    Price = 200000m,
                    LandSize = 200,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Prop 2",
                    AgentId = "a2",
                    Status = PropertyStatus.Sold
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = query.Where(p => p.Status == PropertyStatus.Available).ToList();

            // Assert
            result.Should().ContainSingle();
            result[0].Code.Should().Be("AVAILABLE1");
        }

    }
}
