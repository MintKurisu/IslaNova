using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Queries.GetAvailableProperties;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Core.Application.Mappings.EntityToDtos.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class GetAvailablePropertiesQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAvailablePropertiesQueryHandlerTest()
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
        public async Task Handle_Should_Return_Only_Available_Properties()
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
                    Description = "Available Property",
                    AgentId = "00000000000000001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 2,
                    SaleTypeId = 2,
                    Price = 45000.00m,
                    LandSize = 1300.5,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "Sold Property",
                    AgentId = "00000000000000002",
                    Status = PropertyStatus.Sold,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(a => a.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new Core.Application.Dtos.User.UserDto
                {
                    Id = "00000000000000001",
                    Name = "Joe",
                    LastName = "Doe",
                    IdentificationNumber = "01234567891",
                    Email = "joe@example.com",
                    UserName = "jdoe",
                    PhoneNumber = "555-1234",
                    Role = "Agent"
                });

            var handler = new GetAvailablePropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new GetAvailablePropertiesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PropertyId.Should().Be(1);
            result[0].Description.Should().Be("Available Property");
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Available_Properties()
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
                    Description = "Sold Property",
                    AgentId = "00000000000000001",
                    Status = PropertyStatus.Sold,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new GetAvailablePropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new GetAvailablePropertiesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
