using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Features.AgentApplication.Queries.GetAgentApplicationById;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentApplication
{
    public class GetAgentApplicationByIdQueryHandlerTest
    {
        private readonly Mock<IAgentApplicationRepository> _repositoryMock;
        private readonly Mock<IAuthServiceForWebApi> _authServiceMock;
        private readonly IMapper _mapper;

        public GetAgentApplicationByIdQueryHandlerTest()
        {
            _repositoryMock = new Mock<IAgentApplicationRepository>();
            _authServiceMock = new Mock<IAuthServiceForWebApi>();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<AgentApplicationMappingProfile>();
            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_ApplicationNotFound_ReturnsNull()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            var handler = new GetAgentApplicationByIdQueryHandler(
                _repositoryMock.Object, _authServiceMock.Object, _mapper);

            var query = new GetAgentApplicationByIdQuery { ApplicationId = 99 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _authServiceMock.Verify(a => a.GetUserById(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ApplicationFound_AgentFoundInIdentity_ReturnsDtoWithAgentInfo()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 1,
                UserId = "user-001",
                ProfessionalStatement = "Experienced professional",
                EmploymentType = EmploymentType.Independent,
                Status = ApplicationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            var agentDto = new UserDto
            {
                Id = "user-001",
                Name = "María",
                LastName = "López",
                Email = "maria@example.com",
                PhoneNumber = "8091112233",
                IdentificationNumber = "00112233445",
                UserName = "mlopez",
                Role = "Agent"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            _authServiceMock
                .Setup(a => a.GetUserById("user-001"))
                .ReturnsAsync(agentDto);

            var handler = new GetAgentApplicationByIdQueryHandler(
                _repositoryMock.Object, _authServiceMock.Object, _mapper);

            var query = new GetAgentApplicationByIdQuery { ApplicationId = 1 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ApplicationId.Should().Be(1);
            result.UserId.Should().Be("user-001");
            result.AgentName.Should().Be("María López");
            result.AgentEmail.Should().Be("maria@example.com");
            result.AgentPhone.Should().Be("8091112233");
            result.Status.Should().Be(ApplicationStatus.Pending.ToString());
        }

        [Fact]
        public async Task Handle_ApplicationFound_AgentNotFoundInIdentity_ReturnsDtoWithoutAgentInfo()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 2,
                UserId = "user-orphaned",
                ProfessionalStatement = "Orphaned application",
                EmploymentType = EmploymentType.Agency,
                Status = ApplicationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(application);

            // El usuario ya no existe en Identity
            _authServiceMock
                .Setup(a => a.GetUserById("user-orphaned"))
                .ReturnsAsync((UserDto?)null);

            var handler = new GetAgentApplicationByIdQueryHandler(
                _repositoryMock.Object, _authServiceMock.Object, _mapper);

            var query = new GetAgentApplicationByIdQuery { ApplicationId = 2 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.ApplicationId.Should().Be(2);
            result.AgentName.Should().BeNull();
            result.AgentEmail.Should().BeNull();
            result.AgentPhone.Should().BeNull();
        }
    }
}
