using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Features.Property.Commands.DeleteProperty;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class DeletePropertyCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public DeletePropertyCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_Property_And_Related_Images_And_Improvements()
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

            context.Improvements.Add(
                new Core.Domain.Entities.Feature.Improvement
                { ImprovementId = 1, Name = "Pool", Description = "Pool Description" }
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
                    Description = "Property to delete",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.PropertyImages.AddRange(
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 1, PropertyId = 1, ImageUrl = "https://example.com/image1.jpg" },
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 2, PropertyId = 1, ImageUrl = "https://example.com/image2.jpg" }
            );

            context.PropertyImprovements.Add(
                new PropertyImprovement
                { PropertyImprovementId = 1, PropertyId = 1, ImprovementId = 1 }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var handler = new DeletePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository
            );

            var command = new DeletePropertyCommand
            {
                PropertyId = 1
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deletedProperty = await context.Properties.FindAsync(1);
            deletedProperty.Should().BeNull();

            var images = await context.PropertyImages.Where(img => img.PropertyId == 1).ToListAsync();
            images.Should().BeEmpty();

            var improvements = await context.PropertyImprovements.Where(pi => pi.PropertyId == 1).ToListAsync();
            improvements.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_Delete_Only_Specified_Property()
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

            context.Properties.AddRange(
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
                    Description = "Property to delete",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 45000.00m,
                    LandSize = 3000.0,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "Property to keep",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var handler = new DeletePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository
            );

            var command = new DeletePropertyCommand
            {
                PropertyId = 1
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deletedProperty = await context.Properties.FindAsync(1);
            deletedProperty.Should().BeNull();

            var remainingProperty = await context.Properties.FindAsync(2);
            remainingProperty.Should().NotBeNull();
        }
    }
}
