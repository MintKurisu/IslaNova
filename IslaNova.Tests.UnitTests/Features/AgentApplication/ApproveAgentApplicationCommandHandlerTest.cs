using FluentAssertions;
using Moq;
using IslaNova.Core.Application.Features.AgentApplication.Commands.ApproveAgentApplication;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentApplication
{
    public class ApproveAgentApplicationCommandHandlerTest
    {
        private readonly Mock<IAgentApplicationRepository> _repositoryMock;
        private readonly Mock<IAuthServiceForWebApi> _authServiceMock;

        public ApproveAgentApplicationCommandHandlerTest()
        {
            _repositoryMock = new Mock<IAgentApplicationRepository>();
            _authServiceMock = new Mock<IAuthServiceForWebApi>();
        }

        [Fact]
        public async Task Handle_ApplicationNotFound_ReturnsFalse()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            var handler = new ApproveAgentApplicationCommandHandler(
                _repositoryMock.Object, _authServiceMock.Object);

            var command = new ApproveAgentApplicationCommand { ApplicationId = 99 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()), Times.Never);
            _authServiceMock.Verify(a => a.ToggleUserStatus(It.IsAny<string>(), It.IsAny<bool?>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ApplicationNotPending_ReturnsFalse()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 1,
                UserId = "user-001",
                ProfessionalStatement = "Statement",
                Status = ApplicationStatus.Approved   // Ya fue aprobada
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var handler = new ApproveAgentApplicationCommandHandler(
                _repositoryMock.Object, _authServiceMock.Object);

            var command = new ApproveAgentApplicationCommand { ApplicationId = 1 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()), Times.Never);
            _authServiceMock.Verify(a => a.ToggleUserStatus(It.IsAny<string>(), It.IsAny<bool?>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RejectedApplication_ReturnsFalse()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 2,
                UserId = "user-002",
                ProfessionalStatement = "Statement",
                Status = ApplicationStatus.Rejected
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(application);

            var handler = new ApproveAgentApplicationCommandHandler(
                _repositoryMock.Object, _authServiceMock.Object);

            var command = new ApproveAgentApplicationCommand { ApplicationId = 2 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PendingApplication_ApprovesAndActivatesUser()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 3,
                UserId = "user-003",
                ProfessionalStatement = "I am a seasoned real estate professional.",
                Status = ApplicationStatus.Pending
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(3))
                .ReturnsAsync(application);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()))
                .ReturnsAsync(application);

            _authServiceMock
                .Setup(a => a.ToggleUserStatus("user-003", true))
                .ReturnsAsync(true);

            var handler = new ApproveAgentApplicationCommandHandler(
                _repositoryMock.Object, _authServiceMock.Object);

            var command = new ApproveAgentApplicationCommand
            {
                ApplicationId = 3,
                ReviewedBy = "admin-001"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            // Verificar que se actualizó el estado de la aplicación
            _repositoryMock.Verify(r => r.UpdateAsync(3, It.Is<Core.Domain.Entities.AccountManagement.AgentApplication>(
                a => a.Status == ApplicationStatus.Approved &&
                     a.ReviewedBy == "admin-001" &&
                     a.ReviewedAt != null
            )), Times.Once);

            // Verificar que se activó la cuenta del usuario
            _authServiceMock.Verify(a => a.ToggleUserStatus("user-003", true), Times.Once);
        }
    }
}
