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
    public class OfferRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public OfferRepositoryTests()
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
        public async Task AddAsync_Should_Add_Offer_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new OfferRepository(context);

            var offer = new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "client123",
                Amount = 240000m
            };

            // Act
            var result = await repository.AddAsync(offer);

            // Assert
            result.Should().NotBeNull();
            result!.OfferId.Should().BeGreaterThan(0);
            result.Amount.Should().Be(240000m);
            result.ClientId.Should().Be("client123");
            result.PropertyId.Should().Be(property.PropertyId);
            result.Status.Should().Be(OfferStatus.Pending);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var offersInDb = await context.Offers.ToListAsync();
            offersInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new OfferRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Offer_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new OfferRepository(context);

            var offer = new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "client456",
                Amount = 235000m
            };
            offer = await repository.AddAsync(offer);

            // Act
            var result = await repository.GetByIdAsync(offer!.OfferId);

            // Assert
            result.Should().NotBeNull();
            result!.OfferId.Should().Be(offer.OfferId);
            result.Amount.Should().Be(235000m);
            result.ClientId.Should().Be("client456");
            result.Status.Should().Be(OfferStatus.Pending);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new OfferRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Offer_In_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new OfferRepository(context);

            var offer = new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "client789",
                Amount = 230000m,
                Status = OfferStatus.Pending
            };
            offer = await repository.AddAsync(offer);
            offer!.Status = OfferStatus.Accepted;

            // Act
            var updated = await repository.UpdateAsync(offer.OfferId, offer);

            // Assert
            updated.Should().NotBeNull();
            updated!.Status.Should().Be(OfferStatus.Accepted);

            var fromDb = await repository.GetByIdAsync(offer.OfferId);
            fromDb!.Status.Should().Be(OfferStatus.Accepted);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Offer_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new OfferRepository(context);

            var fakeOffer = new Offer
            {
                OfferId = 9999,
                PropertyId = 1,
                ClientId = "fake",
                Amount = 100000m
            };

            // Act
            var updated = await repository.UpdateAsync(9999, fakeOffer);

            // Assert
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Offer()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new OfferRepository(context);

            var offer = new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "client999",
                Amount = 245000m
            };
            offer = await repository.AddAsync(offer);

            // Act
            await repository.DeleteAsync(offer!.OfferId);

            // Assert
            var entity = await repository.GetByIdAsync(offer.OfferId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new OfferRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Offers()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.Offers.AddRange(
                new Offer { PropertyId = property.PropertyId, ClientId = "client1", Amount = 230000m },
                new Offer { PropertyId = property.PropertyId, ClientId = "client2", Amount = 240000m },
                new Offer { PropertyId = property.PropertyId, ClientId = "client3", Amount = 235000m, Status = OfferStatus.Rejected }
            );
            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_Offers()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new OfferRepository(context);

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
            var repository = new OfferRepository(context);

            var offer = new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "clientinclude",
                Amount = 250000m
            };
            offer = await repository.AddAsync(offer);

            // Act
            var result = await repository.GetByIdWithIncludeAsync(
                offer!.OfferId,
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
            var property = await CreateTestProperty(context);

            context.Offers.AddRange(
                new Offer { PropertyId = property.PropertyId, ClientId = "c1", Amount = 230000m, Status = OfferStatus.Pending },
                new Offer { PropertyId = property.PropertyId, ClientId = "c2", Amount = 240000m, Status = OfferStatus.Accepted },
                new Offer { PropertyId = property.PropertyId, ClientId = "c3", Amount = 235000m, Status = OfferStatus.Pending }
            );
            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = query.Where(o => o.Status == OfferStatus.Pending).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(o => o.Status == OfferStatus.Pending);
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.Offers.Add(new Offer
            {
                PropertyId = property.PropertyId,
                ClientId = "client1",
                Amount = 240000m
            });
            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);

            // Act
            var query = repository.GetAllQueryWithInclude(new List<string> { "Property" });
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Property.Should().NotBeNull();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Offers()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new OfferRepository(context);

            var offers = new List<Offer>
            {
                new Offer { PropertyId = property.PropertyId, ClientId = "client1", Amount = 230000m },
                new Offer { PropertyId = property.PropertyId, ClientId = "client2", Amount = 240000m },
                new Offer { PropertyId = property.PropertyId, ClientId = "client3", Amount = 235000m }
            };

            // Act
            var result = await repository.AddRangeAsync(offers);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(o => o.OfferId > 0);
        }
    }
}
