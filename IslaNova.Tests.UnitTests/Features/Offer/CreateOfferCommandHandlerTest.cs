using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.Offer.Commands.CreateOffer;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;
using IslaNova.Core.Application.Mappings.EntityToDtos.OfferManagement;

namespace IslaNova.Tests.UnitTests.Features.Offer
{
    public class CreateOfferCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreateOfferCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<OfferMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Create_Offer_When_No_Accepted_Offer_Exists()
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

            var repository = new OfferRepository(context);
            var handler = new CreateOfferCommandHandler(repository, _mapper);

            var command = new CreateOfferCommand
            {
                PropertyId = 1,
                ContactName = "John Doe",
                ContactPhone = "+1234567890",
                ContactEmail = "john@example.com",
                Amount = 30000.00m
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ContactName.Should().Be(command.ContactName);
            result.Amount.Should().Be(command.Amount);
            result.PropertyId.Should().Be(command.PropertyId);

            var createdOffer = await context.Offers.FindAsync(result.OfferId);
            createdOffer.Should().NotBeNull();
            createdOffer!.Status.Should().Be(OfferStatus.Pending);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Accepted_Offer_Exists()
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
                    PropertyId = 1,
                    ContactName = "Jane Doe",
                    ContactPhone = "+9876543210",
                    Amount = 35000.00m,
                    Status = OfferStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new CreateOfferCommandHandler(repository, _mapper);

            var command = new CreateOfferCommand
            {
                PropertyId = 1,
                ContactName = "John Doe",
                ContactPhone = "+1234567890",
                Amount = 30000.00m
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
