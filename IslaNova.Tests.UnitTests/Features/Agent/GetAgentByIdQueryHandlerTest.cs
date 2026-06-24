using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using IslaNova.Core.Application.Features.Agent.Queries.GetById;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Agent
{
    public class GetAgentByIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public GetAgentByIdQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }


        [Fact]
        public async Task Handle_Should_Return_Agent_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            context.Properties.Add(
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
                    AgentId = "001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            var user = new Core.Application.Dtos.User.UserDto
            {
                Id = "001",
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

            var handler = new GetAgentByIdQueryHandler(authServiceMock.Object, repository);

            // Act
            var result = await handler.Handle(new GetAgentByIdQuery { Id = "001" }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Joe");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("joe.doe@example.com");
            result.PhoneNumber.Should().Be("809-555-1234");

        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Property_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new GetAgentByIdQueryHandler(authServiceMock.Object, repository);

            // Act
            var result = await handler.Handle(new GetAgentByIdQuery() { Id = "" }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
