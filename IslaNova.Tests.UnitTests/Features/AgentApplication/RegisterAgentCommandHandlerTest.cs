using FluentAssertions;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Identity.Entities;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.RegisterAgent;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace IslaNova.Tests.UnitTests.Features.AgentApplication
{
    public class RegisterAgentCommandHandlerTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IAgentApplicationRepository> _repositoryMock;
        private readonly Mock<IStorageService> _storageServiceMock;

        public RegisterAgentCommandHandlerTest()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _repositoryMock = new Mock<IAgentApplicationRepository>();
            _storageServiceMock = new Mock<IStorageService>();
        }

        private RegisterAgentCommand BuildValidCommand() => new()
        {
            Name = "Carlos",
            LastName = "Pérez",
            Email = "carlos@test.com",
            PhoneNumber = "8091234567",
            IdentificationNumber = "00112233445",
            Password = "Password123!",
            LicenseNumber = "LIC-12345",
            ProfessionalStatement = "Tengo 5 años de experiencia en bienes raíces.",
            EmploymentType = EmploymentType.Independent
        };

        [Fact]
        public async Task Handle_EmailAlreadyExists_ReturnsError()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(command.Email!))
                .ReturnsAsync(new User { IdentificationNumber = "", Name = "", LastName = "" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeTrue();
            result.Errors.Should().Contain("Email already exists.");
        }

        [Fact]
        public async Task Handle_IdentificationNumberAlreadyExists_ReturnsError()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock
                .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _userManagerMock
                .Setup(u => u.Users)
                .Returns(new List<User>
                {
                new() { IdentificationNumber = command.IdentificationNumber, Name = "", LastName = "" }
                }.AsQueryable());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeTrue();
            result.Errors.Should().Contain("Identification number already exists.");
        }

        [Fact]
        public async Task Handle_CreateUserFails_ReturnsIdentityErrors()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());

            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeTrue();
            result.Errors.Should().Contain("Password too weak.");
        }

        [Fact]
        public async Task Handle_ApplicationAlreadyExists_RollsBackAndReturnsError()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            // Una aplicación ya existe para ese usuario
            _repositoryMock
                .Setup(r => r.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    UserId = "existing-user-id",
                    ProfessionalStatement = "Existing application"
                });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeTrue();
            result.Errors.Should().Contain("This user already has an agent application.");

            // Verificar que se hizo rollback del usuario
            _userManagerMock.Verify(u => u.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ApplicationCreationFails_RollsBackAndReturnsError()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            // No hay aplicación previa
            _repositoryMock
                .Setup(r => r.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            // El repositorio falla al guardar (retorna null)
            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeTrue();
            result.Errors.Should().Contain("Failed to create agent application. Registration rolled back.");

            // Verificar que se hizo rollback del usuario
            _userManagerMock.Verify(u => u.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesUserAndApplicationSuccessfully()
        {
            // Arrange
            var command = BuildValidCommand();
            var handler = new RegisterAgentCommandHandler(
                _userManagerMock.Object,
                _repositoryMock.Object,
                _storageServiceMock.Object
            );

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            _repositoryMock
                .Setup(r => r.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentApplication?)null);

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()))
                .ReturnsAsync(new Core.Domain.Entities.AccountManagement.AgentApplication
                {
                    ApplicationId = 1,
                    UserId = "new-user-id",
                    ProfessionalStatement = command.ProfessionalStatement!,
                    Status = ApplicationStatus.Pending
                });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.HasError.Should().BeFalse();
            result.Errors.Should().BeEmpty();

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Core.Domain.Entities.AccountManagement.AgentApplication>()), Times.Once);
        }
    }
}
