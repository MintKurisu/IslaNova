using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Commands.UpdateProperty;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;
using IslaNova.Core.Application.Mappings.EntityToDtos.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class UpdatePropertyCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdatePropertyCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<PropertyMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Update_Property_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.AddRange(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" },
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 2, Name = "House", Description = "House Description" }
            );

            context.SaleTypes.AddRange(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" },
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 2, Name = "For Sale", Description = "For Sale Description" }
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
                    Description = "Old description",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new UpdatePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository,
                authServiceMock.Object,
                _mapper
            );

            var command = new UpdatePropertyCommand
            {
                PropertyId = 1,
                PropertyTypeId = 2,
                SaleTypeId = 2,
                Price = 50000.00m,
                LandSize = 3000.0,
                Bedrooms = 5,
                Bathrooms = 4,
                Description = "Updated description",
                ImprovementIds = new List<int>(),
                ImageUrls = new List<string>(),
                Latitude = 18.5,
                Longitude = -69.9,
                Address = "Updated Address",
                City = "Updated City"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyTypeId.Should().Be(2);
            result.SaleTypeId.Should().Be(2);
            result.Price.Should().Be(50000.00m);
            result.Bedrooms.Should().Be(5);
            result.Bathrooms.Should().Be(4);
            result.Description.Should().Be("Updated description");

            var updatedProperty = await context.Properties.FindAsync(1);
            updatedProperty.Should().NotBeNull();
            updatedProperty!.Price.Should().Be(50000.00m);
            updatedProperty.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Property_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new UpdatePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository,
                authServiceMock.Object,
                _mapper
            );

            var command = new UpdatePropertyCommand
            {
                PropertyId = 999,
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 50000.00m,
                LandSize = 3000.0,
                Bedrooms = 5,
                Bathrooms = 4
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Update_Images_When_Provided()
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
                    Description = "Property",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.PropertyImages.AddRange(
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 1, PropertyId = 1, ImageUrl = "https://example.com/old1.jpg" },
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 2, PropertyId = 1, ImageUrl = "https://example.com/old2.jpg" }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new UpdatePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository,
                authServiceMock.Object,
                _mapper
            );

            var command = new UpdatePropertyCommand
            {
                PropertyId = 1,
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 35000.00m,
                LandSize = 2500.5,
                Bedrooms = 4,
                Bathrooms = 3,
                ImageUrls = new List<string> 
                { 
                    "https://example.com/new1.jpg",
                    "https://example.com/new2.jpg",
                    "https://example.com/new3.jpg"
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var images = await context.PropertyImages
                .Where(img => img.PropertyId == 1)
                .ToListAsync();

            images.Should().HaveCount(3);
        }
    }
}
