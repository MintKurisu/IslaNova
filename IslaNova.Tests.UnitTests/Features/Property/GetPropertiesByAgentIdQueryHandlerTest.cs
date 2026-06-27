using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Queries.GetPropertiesByAgentId;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class GetPropertiesByAgentIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertiesByAgentIdQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Properties_For_Agent()
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
                    Description = "Agent 1 Property 1",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
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
                    LandSize = 3000.0,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Agent 2 Property",
                    AgentId = "agent002",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(a => a.GetUserById("agent001"))
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

            var handler = new GetPropertiesByAgentIdQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new GetPropertiesByAgentIdQuery
            {
                AgentId = "agent001"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].PropertyId.Should().Be(2);
            result[1].PropertyId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_Agent_Has_No_Properties()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(a => a.GetUserById("agent_without_properties"))
                .ReturnsAsync(new Core.Application.Dtos.User.UserDto
                {
                    Id = "agent_without_properties",
                    Name = "Test",
                    LastName = "Agent",
                    IdentificationNumber = "01234567890",
                    Email = "test@example.com",
                    UserName = "tagent",
                    PhoneNumber = "555-5678",
                    Role = "Agent"
                });

            var handler = new GetPropertiesByAgentIdQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new GetPropertiesByAgentIdQuery
            {
                AgentId = "agent_without_properties"
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
