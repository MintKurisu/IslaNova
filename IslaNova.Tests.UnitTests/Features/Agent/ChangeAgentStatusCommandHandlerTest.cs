using FluentAssertions;
using Moq;
using IslaNova.Core.Application.Features.Agent.Commands.ChangeAgentStatus;
using IslaNova.Core.Application.Interfaces.Auth;

namespace IslaNova.Tests.UnitTests.Features.Agent
{
    public class ChangeAgentStatusCommandHandlerTest
    {
        [Fact]
        public async Task Handle_Should_Change_Agent_Status_To_Active()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var agentId = "agent001";
            var agent = new Core.Application.Dtos.User.UserDto
            {
                Id = agentId,
                Name = "Jose",
                LastName = "Garcia",
                IdentificationNumber = "01234567890",
                Email = "jose@example.com",
                UserName = "jgarcia",
                PhoneNumber = "555-1234",
                Role = "Agent",
                IsActive = false
            };

            authServiceMock
                .Setup(a => a.GetUserById(agentId))
                .ReturnsAsync(agent);

            authServiceMock
                .Setup(a => a.ToggleUserStatus(agentId, true))
                .ReturnsAsync(true);

            var handler = new ChangeAgentStatusCommandHandler(authServiceMock.Object);

            var command = new ChangeAgentStatusCommand
            {
                Id = agentId,
                Status = true
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(MediatR.Unit.Value);
            authServiceMock.Verify(a => a.GetUserById(agentId), Times.Once);
            authServiceMock.Verify(a => a.ToggleUserStatus(agentId, true), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Change_Agent_Status_To_Inactive()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var agentId = "agent001";
            var agent = new Core.Application.Dtos.User.UserDto
            {
                Id = agentId,
                Name = "Jose",
                LastName = "Garcia",
                IdentificationNumber = "01234567890",
                Email = "jose@example.com",
                UserName = "jgarcia",
                PhoneNumber = "555-1234",
                Role = "Agent",
                IsActive = true
            };

            authServiceMock
                .Setup(a => a.GetUserById(agentId))
                .ReturnsAsync(agent);

            authServiceMock
                .Setup(a => a.ToggleUserStatus(agentId, false))
                .ReturnsAsync(true);

            var handler = new ChangeAgentStatusCommandHandler(authServiceMock.Object);

            var command = new ChangeAgentStatusCommand
            {
                Id = agentId,
                Status = false
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(MediatR.Unit.Value);
            authServiceMock.Verify(a => a.GetUserById(agentId), Times.Once);
            authServiceMock.Verify(a => a.ToggleUserStatus(agentId, false), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Agent_Not_Found()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var agentId = "nonexistent_agent";

            authServiceMock
                .Setup(a => a.GetUserById(agentId))
                .ReturnsAsync((Core.Application.Dtos.User.UserDto)null);

            var handler = new ChangeAgentStatusCommandHandler(authServiceMock.Object);

            var command = new ChangeAgentStatusCommand
            {
                Id = agentId,
                Status = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<Core.Application.Exceptions.ApiException>(
                async () => await handler.Handle(command, CancellationToken.None)
            );

            authServiceMock.Verify(a => a.GetUserById(agentId), Times.Once);
            authServiceMock.Verify(a => a.ToggleUserStatus(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_AgentId_Is_Null()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            authServiceMock
                .Setup(a => a.GetUserById(""))
                .ReturnsAsync((Core.Application.Dtos.User.UserDto)null);

            var handler = new ChangeAgentStatusCommandHandler(authServiceMock.Object);

            var command = new ChangeAgentStatusCommand
            {
                Id = null,
                Status = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<Core.Application.Exceptions.ApiException>(
                async () => await handler.Handle(command, CancellationToken.None)
            );
        }
    }
}
