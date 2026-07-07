using FluentAssertions;
using Moq;
using IslaNova.Core.Application.Features.AgentApplication.Commands.RejectAgentApplication;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentApplication
{
    public class RejectAgentApplicationCommandHandlerTest
    {
        private readonly Mock<IAgentApplicationRepository> _repositoryMock;

        public RejectAgentApplicationCommandHandlerTest()
        {
            _repositoryMock = new Mock<IAgentApplicationRepository>();
        }

        [Fact]
        public async Task Handle_ApplicationNotFound_ReturnsFalse()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            var handler = new RejectAgentApplicationCommandHandler(_repositoryMock.Object);

            var command = new RejectAgentApplicationCommand { ApplicationId = 99 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()), Times.Never);
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
                Status = ApplicationStatus.Approved   // No está en Pending
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var handler = new RejectAgentApplicationCommandHandler(_repositoryMock.Object);

            var command = new RejectAgentApplicationCommand { ApplicationId = 1 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AlreadyRejectedApplication_ReturnsFalse()
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

            var handler = new RejectAgentApplicationCommandHandler(_repositoryMock.Object);

            var command = new RejectAgentApplicationCommand { ApplicationId = 2 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PendingApplication_RejectsWithComments()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 3,
                UserId = "user-003",
                ProfessionalStatement = "I want to be an agent.",
                Status = ApplicationStatus.Pending
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(3))
                .ReturnsAsync(application);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()))
                .ReturnsAsync(application);

            var handler = new RejectAgentApplicationCommandHandler(_repositoryMock.Object);

            var command = new RejectAgentApplicationCommand
            {
                ApplicationId = 3,
                AdminComments = "Insufficient credentials provided.",
                ReviewedBy = "admin-001"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            _repositoryMock.Verify(r => r.UpdateAsync(3, It.Is<Core.Domain.Entities.AccountManagement.AgentApplication>(
                a => a.Status == ApplicationStatus.Rejected &&
                     a.AdminComments == "Insufficient credentials provided." &&
                     a.ReviewedBy == "admin-001" &&
                     a.ReviewedAt != null
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_PendingApplication_RejectsWithoutComments()
        {
            // Arrange
            var application = new Core.Domain.Entities.AccountManagement.AgentApplication
            {
                ApplicationId = 4,
                UserId = "user-004",
                ProfessionalStatement = "Looking to start in real estate.",
                Status = ApplicationStatus.Pending
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(4))
                .ReturnsAsync(application);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()))
                .ReturnsAsync(application);

            var handler = new RejectAgentApplicationCommandHandler(_repositoryMock.Object);

            var command = new RejectAgentApplicationCommand
            {
                ApplicationId = 4,
                AdminComments = null,
                ReviewedBy = "admin-002"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            _repositoryMock.Verify(r => r.UpdateAsync(4, It.Is<Core.Domain.Entities.AccountManagement.AgentApplication>(
                a => a.Status == ApplicationStatus.Rejected &&
                     a.AdminComments == null
            )), Times.Once);
        }
    }
}
