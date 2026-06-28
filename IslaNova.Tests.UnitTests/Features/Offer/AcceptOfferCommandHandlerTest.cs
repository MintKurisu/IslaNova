using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Features.Offer.Commands.AcceptOffer;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;

namespace IslaNova.Tests.UnitTests.Features.Offer
{
    public class AcceptOfferCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public AcceptOfferCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Accept_Offer_And_Reject_Pending_Offers()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.Add(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 35000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 1 Description",
                    AgentId = "00000000000000000",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.Offers.AddRange(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "John Doe",
                    ContactPhone = "+1234567890",
                    Amount = 30000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 2,
                    PropertyId = 1,
                    ContactName = "Jane Smith",
                    ContactPhone = "+9876543210",
                    Amount = 31000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 3,
                    PropertyId = 1,
                    ContactName = "Bob Johnson",
                    ContactPhone = "+5555555555",
                    Amount = 32000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var offerRepository = new OfferRepository(context);
            var propertyRepository = new PropertyRepository(context);
            var handler = new AcceptOfferCommandHandler(offerRepository, propertyRepository);

            var command = new AcceptOfferCommand
            {
                OfferId = 1,
                PropertyId = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            var acceptedOffer = await context.Offers.FindAsync(1);
            acceptedOffer?.Status.Should().Be(OfferStatus.Accepted);

            var rejectedOffer2 = await context.Offers.FindAsync(2);
            rejectedOffer2?.Status.Should().Be(OfferStatus.Rejected);

            var rejectedOffer3 = await context.Offers.FindAsync(3);
            rejectedOffer3?.Status.Should().Be(OfferStatus.Rejected);

            var property = await context.Properties.FindAsync(1);
            property?.Status.Should().Be(PropertyStatus.Sold);
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Offer_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.Add(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 35000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 1 Description",
                    AgentId = "00000000000000000",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var offerRepository = new OfferRepository(context);
            var propertyRepository = new PropertyRepository(context);
            var handler = new AcceptOfferCommandHandler(offerRepository, propertyRepository);

            var command = new AcceptOfferCommand
            {
                OfferId = 999,
                PropertyId = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Offer_Not_Pending()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.Add(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 35000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 1 Description",
                    AgentId = "00000000000000000",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.Offers.Add(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "John Doe",
                    ContactPhone = "+1234567890",
                    Amount = 30000.00m,
                    Status = OfferStatus.Rejected,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var offerRepository = new OfferRepository(context);
            var propertyRepository = new PropertyRepository(context);
            var handler = new AcceptOfferCommandHandler(offerRepository, propertyRepository);

            var command = new AcceptOfferCommand
            {
                OfferId = 1,
                PropertyId = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}
