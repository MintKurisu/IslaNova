using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertyById;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Core.Application.Mappings.EntityToDtos.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class GetPropertyByIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertyByIdQueryHandlerTest()
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
        public async Task Handle_Should_Return_Property_When_Exists()
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

            var property = new Core.Domain.Entities.PropertyManagement.Property
            {
                PropertyId = 1,
                Code = "000123",
                PropertyTypeId = 2,
                SaleTypeId = 1,
                Price = 35000.00m,
                LandSize = 2500.5,
                Bedrooms = 4,
                Bathrooms = 3,
                Description = "Property Description",
                AgentId = "00000000000000000",
                Status = PropertyStatus.Available,
                CreatedAt = DateTime.UtcNow
            };

            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            var user = new Core.Application.Dtos.User.UserDto
            {
                Id = "00000000000000000",
                Name = "Joe",
                LastName = "Doe",
                IdentificationNumber = "01234567891",
                Email = "joe.doe@example.com",
                PhoneNumber = "809-555-1234",
                Role = "Agent",
                UserName = "jdoe"
            };
            authServiceMock.Setup(a => a.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(user);

            var handler = new GetPropertyByIdQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByIdQuery() { PropertyId = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.PropertyId.Should().Be(property.PropertyId);
            result.Code.Should().Be(property.Code);
            result.Bathrooms.Should().Be(property.Bathrooms);
            result.Bedrooms.Should().Be(property.Bedrooms);
            result.Description.Should().Be(property.Description);
            result.Price.Should().Be(property.Price);
            result.AgentName.Should().Be(user.Name + " " + user.LastName);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Property_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new PropertyRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(a => a.GetUserById(It.IsAny<string>()))
                .ReturnsAsync(new Core.Application.Dtos.User.UserDto
                {
                    Id = "00000000000000000",
                    Name = "Joe",
                    LastName = "Doe",
                    IdentificationNumber = "01234567891",
                    Email = "joe.doe@example.com",
                    PhoneNumber = "555-1234",
                    Role = "Agent",
                    UserName = "jdoe"
                });

            var handler = new GetPropertyByIdQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyByIdQuery() { PropertyId = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

    }
}
