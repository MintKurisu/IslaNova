using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Features.Property.Commands.DeletePropertiesByAgent;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class DeletePropertiesByAgentCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public DeletePropertiesByAgentCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_All_Properties_For_Agent()
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
                    Description = "Agent 1 Property 1",
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
                    Description = "Agent 1 Property 2",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 3,
                    Code = "000125",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 55000.00m,
                    LandSize = 2000.0,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Agent 2 Property",
                    AgentId = "agent002",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.PropertyImages.AddRange(
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 1, PropertyId = 1, ImageUrl = "https://example.com/image1.jpg" },
                new Core.Domain.Entities.PropertyManagement.PropertyImage
                { PropertyImageId = 2, PropertyId = 2, ImageUrl = "https://example.com/image2.jpg" }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var handler = new DeletePropertiesByAgentCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository
            );

            var command = new DeletePropertiesByAgentCommand
            {
                AgentId = "agent001"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var agent1Properties = await context.Properties.Where(p => p.AgentId == "agent001").ToListAsync();
            agent1Properties.Should().BeEmpty();

            var agent2Property = await context.Properties.FindAsync(3);
            agent2Property.Should().NotBeNull();

            var images = await context.PropertyImages.Where(img => img.PropertyId == 1 || img.PropertyId == 2).ToListAsync();
            images.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_Do_Nothing_When_Agent_Has_No_Properties()
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
                    Description = "Agent 2 Property",
                    AgentId = "agent002",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var handler = new DeletePropertiesByAgentCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository
            );

            var command = new DeletePropertiesByAgentCommand
            {
                AgentId = "agent001"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var allProperties = await context.Properties.ToListAsync();
            allProperties.Should().HaveCount(1);
            allProperties[0].PropertyId.Should().Be(1);
        }
    }
}
