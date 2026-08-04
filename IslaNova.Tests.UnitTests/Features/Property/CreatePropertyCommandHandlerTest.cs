using AutoMapper;
using FluentAssertions;
using IslaNova.Core.Application.Features.Property.Commands.CreateProperty;
using IslaNova.Core.Application.Features.Property.Events;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos.PropertyManagement;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Channels;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class CreatePropertyCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreatePropertyCommandHandlerTest()
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
        public async Task Handle_Should_Create_Property_With_Valid_Command()
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

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(a => a.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new Core.Application.Dtos.User.UserDto
                {
                    Id = "agent001",
                    Name = "Jose",
                    LastName = "Garcia",
                    IdentificationNumber = "01234567890",
                    Email = "jose@example.com",
                    UserName = "jgarcia",
                    PhoneNumber = "555-1234",
                    Role = "Agent"
                });

            var handler = new CreatePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository,
                authServiceMock.Object,
                _mapper,
                Channel.CreateUnbounded<PropertyVectorEvent>()
            );

            var command = new CreatePropertyCommand
            {
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 35000.00m,
                LandSize = 2500.5,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Beautiful apartment",
                AgentId = "agent001",
                ImageUrls = new List<string> { "https://example.com/image1.jpg" },
                ImprovementIds = new List<int>(),
                Latitude = 18.4861,
                Longitude = -69.9312,
                Address = "123 Main Street",
                City = "Santo Domingo"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyTypeId.Should().Be(1);
            result.SaleTypeId.Should().Be(1);
            result.Price.Should().Be(35000.00m);
            result.Bedrooms.Should().Be(4);
            result.Bathrooms.Should().Be(3);
            result.Description.Should().Be("Beautiful apartment");

            var createdProperty = await context.Properties.FindAsync(result.PropertyId);
            createdProperty.Should().NotBeNull();
            createdProperty!.Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task Handle_Should_Generate_Unique_Code()
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

            await context.SaveChangesAsync();

            var propertyRepository = new PropertyRepository(context);
            var propertyImageRepository = new PropertyImageRepository(context);
            var propertyImprovementRepository = new PropertyImprovementRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new CreatePropertyCommandHandler(
                propertyRepository,
                propertyImageRepository,
                propertyImprovementRepository,
                authServiceMock.Object,
                _mapper,
                Channel.CreateUnbounded<PropertyVectorEvent>()
            );

            var command = new CreatePropertyCommand
            {
                PropertyTypeId = 1,
                SaleTypeId = 1,
                Price = 35000.00m,
                LandSize = 2500.5,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Beautiful apartment",
                AgentId = "agent001",
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().NotBeNullOrEmpty();
            result.Code.Should().HaveLength(6);
        }
    }
}
