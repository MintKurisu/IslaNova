using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Features.AgentApplication.Queries.GetAllAgentApplications;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentApplication
{
    public class GetAllAgentApplicationsQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllAgentApplicationsQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<AgentApplicationMappingProfile>();
            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_NoApplications_ReturnsEmptyList()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new AgentApplicationRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new GetAllAgentApplicationsQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllAgentApplicationsQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
            authServiceMock.Verify(a => a.GetUserById(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithApplications_AgentFoundInIdentity_ReturnsDtosWithAgentInfo()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new AgentApplicationRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            context.AgentApplications.AddRange(
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "user-001",
                    ProfessionalStatement = "Statement 1",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "user-002",
                    ProfessionalStatement = "Statement 2",
                    EmploymentType = EmploymentType.Agency,
                    Status = ApplicationStatus.Approved,
                    SubmittedAt = DateTime.UtcNow.AddDays(-1)
                }
            );
            await context.SaveChangesAsync();

            authServiceMock
                .Setup(a => a.GetUserById("user-001"))
                .ReturnsAsync(new UserDto
                {
                    Id = "user-001",
                    Name = "Ana",
                    LastName = "Martínez",
                    Email = "ana@example.com",
                    PhoneNumber = "8092223344",
                    IdentificationNumber = "00112233445",
                    UserName = "amartinez",
                    Role = "Agent"
                });

            authServiceMock
                .Setup(a => a.GetUserById("user-002"))
                .ReturnsAsync(new UserDto
                {
                    Id = "user-002",
                    Name = "Luis",
                    LastName = "García",
                    Email = "luis@example.com",
                    PhoneNumber = "8093334455",
                    IdentificationNumber = "00223344556",
                    UserName = "lgarcia",
                    Role = "Agent"
                });

            var handler = new GetAllAgentApplicationsQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllAgentApplicationsQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);

            var ana = result.First(r => r.UserId == "user-001");
            ana.AgentName.Should().Be("Ana Martínez");
            ana.AgentEmail.Should().Be("ana@example.com");
            ana.AgentPhone.Should().Be("8092223344");

            var luis = result.First(r => r.UserId == "user-002");
            luis.AgentName.Should().Be("Luis García");
        }

        [Fact]
        public async Task Handle_WithApplications_AgentNotFoundInIdentity_ReturnsDtosWithoutAgentInfo()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new AgentApplicationRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            context.AgentApplications.Add(
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "user-orphaned",
                    ProfessionalStatement = "Orphaned statement",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Pending,
                    SubmittedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            authServiceMock
                .Setup(a => a.GetUserById("user-orphaned"))
                .ReturnsAsync((UserDto?)null);

            var handler = new GetAllAgentApplicationsQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllAgentApplicationsQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);

            var dto = result.First();
            dto.AgentName.Should().BeNull();
            dto.AgentEmail.Should().BeNull();
            dto.AgentPhone.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithApplications_ReturnsListOrderedBySubmittedAtDescending()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new AgentApplicationRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var oldDate = DateTime.UtcNow.AddDays(-5);
            var newDate = DateTime.UtcNow;

            context.AgentApplications.AddRange(
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "user-old",
                    ProfessionalStatement = "Old application",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Pending,
                    SubmittedAt = oldDate
                },
                new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "user-new",
                    ProfessionalStatement = "New application",
                    EmploymentType = EmploymentType.Agency,
                    Status = ApplicationStatus.Pending,
                    SubmittedAt = newDate
                }
            );
            await context.SaveChangesAsync();

            authServiceMock
                .Setup(a => a.GetUserById(It.IsAny<string>()))
                .ReturnsAsync((UserDto?)null);

            var handler = new GetAllAgentApplicationsQueryHandler(repository, authServiceMock.Object, _mapper);

            // Act
            var result = await handler.Handle(new GetAllAgentApplicationsQuery(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().UserId.Should().Be("user-new");
            result.Last().UserId.Should().Be("user-old");
        }
    }
}
