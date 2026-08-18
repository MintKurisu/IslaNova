using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Queries.GetAllProperty;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using IslaNova.Core.Application.Mappings.EntityToDtos.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class GetAllPropertyQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllPropertyQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Property()
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
                    PropertyTypeId = 2,
                    SaleTypeId = 1,
                    Price = 35000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 1 Description",
                    AgentId = "00000000000000000",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 1,
                    SaleTypeId = 2,
                    Price = 45000.00m,
                    LandSize = 1300.5,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "Property 2 Description",
                    AgentId = "00000000000000000",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

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

            var handler = new GetAllPropertyQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data.All(p => p.Code is not null).Should().BeTrue();
            result.Data.All(p => p.AgentName == "Joe Doe").Should().BeTrue();
            result.Data.First(p => p.Code == "000123").Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_PropertyType_Exists()
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

            var handler = new GetAllPropertyQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().BeEmpty();
        }

    }
}
