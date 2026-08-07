using FluentAssertions;
using IslaNova.Core.Application.Features.Agent.Queries.GetAllAgent;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IslaNova.Tests.UnitTests.Features.Agent
{
    public class GetAllAgentQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public GetAllAgentQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }


        [Fact]
        public async Task Handle_Should_Return_All_Agent()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

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
                    AgentId = "001",
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
                    AgentId = "002",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.AgentApplications.AddRange(
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    ApplicationId = 1,
                    UserId = "001",
                    Status = ApplicationStatus.Approved,
                    LicenseNumber = "LIC-001",
                    ProfessionalStatement = "Statement for agent 001"
                },
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    ApplicationId = 2,
                    UserId = "002",
                    Status = ApplicationStatus.Approved,
                    LicenseNumber = "LIC-002",
                    ProfessionalStatement = "Statement for agent 002"
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var agentApplicationRepository = new AgentApplicationRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            // Mock GetAllUserByRole to return a list of users
            authServiceMock
                .Setup(a => a.GetAllUserByRole(Roles.Agent))
                .ReturnsAsync(new List<Core.Application.Dtos.User.UserDto>
                {
                    new Core.Application.Dtos.User.UserDto
                    {
                        Id = "001",
                        Name = "Joe",
                        LastName = "Doe",
                        IdentificationNumber = "01234567891",
                        Email = "joe.doe@example.com",
                        PhoneNumber = "555-1234",
                        Role = "Agent",
                        UserName = "jdoe"
                    },
                    new Core.Application.Dtos.User.UserDto
                    {
                        Id = "002",
                        Name = "Jane",
                        LastName = "Smith",
                        IdentificationNumber = "98765432109",
                        Email = "jane.smith@example.com",
                        PhoneNumber = "555-5678",
                        Role = "Agent",
                        UserName = "jsmith"
                    }
                });

            var handler = new GetAllAgentQueryHandler(authServiceMock.Object, repository, agentApplicationRepository);

            // Act
            var result = await handler.Handle(new GetAllAgentQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().HaveCount(2);

            result.Data.All(a => !string.IsNullOrWhiteSpace(a.Id)).Should().BeTrue();
            result.Data.All(a => !string.IsNullOrWhiteSpace(a.Name)).Should().BeTrue();
            result.Data.All(a => !string.IsNullOrWhiteSpace(a.Email)).Should().BeTrue();
            result.Data.All(a => !string.IsNullOrWhiteSpace(a.PhoneNumber)).Should().BeTrue();

            var joe = result.Data.First(a => a.Id == "001");
            joe.Name.Should().Be("Joe");
            joe.Email.Should().Be("joe.doe@example.com");
            joe.PhoneNumber.Should().Be("555-1234");

            var jane = result.Data.First(a => a.Id == "002");
            jane.Name.Should().Be("Jane");
            jane.Email.Should().Be("jane.smith@example.com");
            jane.PhoneNumber.Should().Be("555-5678");

        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Agent_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyRepository(context);
            var agentApplicationRepository = new AgentApplicationRepository(context);

            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock
               .Setup(a => a.GetAllUserByRole(Roles.Agent))
               .ReturnsAsync([]);

            var handler = new GetAllAgentQueryHandler(authServiceMock.Object, repository, agentApplicationRepository);

            // Act
            var result = await handler.Handle(new GetAllAgentQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().BeEmpty();
            result.Meta.Total.Should().Be(0);
        }
    }
}
