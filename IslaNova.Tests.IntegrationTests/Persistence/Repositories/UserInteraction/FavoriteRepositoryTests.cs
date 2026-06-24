using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Entities.UserInteraction;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;
using Xunit;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.UserInteraction
{
    public class FavoriteRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public FavoriteRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<Property> CreateTestProperty(IslaNovaContext context)
        {
            var propertyType = new PropertyType { Name = "House", Description = "Test" };
            var saleType = new SaleType { Name = "Sale", Description = "Test" };
            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);
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

            return property;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Favorite_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new FavoriteRepository(context);

            var favorite = new Favorite
            {
                ClientId = "client123",
                PropertyId = property.PropertyId
            };

            // Act
            var result = await repository.AddAsync(favorite);

            // Assert
            result.Should().NotBeNull();
            result!.FavoriteId.Should().BeGreaterThan(0);
            result.ClientId.Should().Be("client123");
            result.PropertyId.Should().Be(property.PropertyId);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var favoritesInDb = await context.Favorites.ToListAsync();
            favoritesInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new FavoriteRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Favorite_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new FavoriteRepository(context);

            var favorite = new Favorite
            {
                ClientId = "client456",
                PropertyId = property.PropertyId
            };
            favorite = await repository.AddAsync(favorite);

            // Act
            var result = await repository.GetByIdAsync(favorite!.FavoriteId);

            // Assert
            result.Should().NotBeNull();
            result!.FavoriteId.Should().Be(favorite.FavoriteId);
            result.ClientId.Should().Be("client456");
            result.PropertyId.Should().Be(property.PropertyId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new FavoriteRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Favorite()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new FavoriteRepository(context);

            var favorite = new Favorite
            {
                ClientId = "client789",
                PropertyId = property.PropertyId
            };
            favorite = await repository.AddAsync(favorite);

            // Act
            await repository.DeleteAsync(favorite!.FavoriteId);

            // Assert
            var entity = await repository.GetByIdAsync(favorite.FavoriteId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new FavoriteRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Favorites()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property1 = await CreateTestProperty(context);
            var property2 = await CreateTestProperty(context);

            context.Favorites.AddRange(
                new Favorite { ClientId = "client1", PropertyId = property1.PropertyId },
                new Favorite { ClientId = "client1", PropertyId = property2.PropertyId },
                new Favorite { ClientId = "client2", PropertyId = property1.PropertyId }
            );
            await context.SaveChangesAsync();

            var repository = new FavoriteRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_Favorites()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new FavoriteRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_Should_Include_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new FavoriteRepository(context);

            var favorite = new Favorite
            {
                ClientId = "clientwithinclude",
                PropertyId = property.PropertyId
            };
            favorite = await repository.AddAsync(favorite);

            // Act
            var result = await repository.GetByIdWithIncludeAsync(
                favorite!.FavoriteId,
                new List<string> { "Property" }
            );

            // Assert
            result.Should().NotBeNull();
            result!.Property.Should().NotBeNull();
            result.Property!.PropertyId.Should().Be(property.PropertyId);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property1 = await CreateTestProperty(context);
            var property2 = await CreateTestProperty(context);

            context.Favorites.AddRange(
                new Favorite { ClientId = "specificUser", PropertyId = property1.PropertyId },
                new Favorite { ClientId = "specificUser", PropertyId = property2.PropertyId },
                new Favorite { ClientId = "otherUser", PropertyId = property1.PropertyId }
            );
            await context.SaveChangesAsync();

            var repository = new FavoriteRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = query.Where(f => f.ClientId == "specificUser").ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(f => f.ClientId == "specificUser");
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.Favorites.Add(new Favorite
            {
                ClientId = "client1",
                PropertyId = property.PropertyId
            });
            await context.SaveChangesAsync();

            var repository = new FavoriteRepository(context);

            // Act
            var query = repository.GetAllQueryWithInclude(new List<string> { "Property" });
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Property.Should().NotBeNull();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Favorites()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property1 = await CreateTestProperty(context);
            var property2 = await CreateTestProperty(context);
            var repository = new FavoriteRepository(context);

            var favorites = new List<Favorite>
            {
                new Favorite { ClientId = "client1", PropertyId = property1.PropertyId },
                new Favorite { ClientId = "client1", PropertyId = property2.PropertyId },
                new Favorite { ClientId = "client2", PropertyId = property1.PropertyId }
            };

            // Act
            var result = await repository.AddRangeAsync(favorites);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(f => f.FavoriteId > 0);
        }
    }
}
