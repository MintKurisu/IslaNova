using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;
using IslaNova.Infrastructure.Identity.Features.Auth.Commands.SignUp;

public class SignUpCommandHandlerTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;

    public SignUpCommandHandlerTest()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_UsernameAlreadyTaken_ReturnsError()
    {
        // Arrange
        var command = new SignUpCommand { UserName = "john" };
        var handler = new SignUpCommandHandler(_userManagerMock.Object);

        _userManagerMock.Setup(u => u.FindByNameAsync(command.UserName))
             .ReturnsAsync(new User() { IdentificationNumber = "", LastName = "", Name = "" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain($"Username {command.UserName} is already taken.");
    }

    [Fact]
    public async Task Handle_EmailAlreadyTaken_ReturnsError()
    {
        // Arrange
        var command = new SignUpCommand { UserName = "john", Email = "test@example.com" };
        var handler = new SignUpCommandHandler(_userManagerMock.Object);

        _userManagerMock.Setup(u => u.FindByNameAsync(command.UserName))
            .ReturnsAsync((User?)null);

        _userManagerMock.Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(new User() { IdentificationNumber = "", LastName = "", Name = "" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain($"Email {command.Email} is already taken.");
    }

    [Fact]
    public async Task Handle_InvalidRole_ReturnsError()
    {
        // Arrange
        var command = new SignUpCommand
        {
            UserName = "john",
            Email = "test@example.com",
            Role = "InvalidRole"
        };
        var handler = new SignUpCommandHandler(_userManagerMock.Object);

        _userManagerMock.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain("Invalid Role.");
    }

    [Fact]
    public async Task Handle_CreateUserFails_ReturnsIdentityErrors()
    {
        // Arrange
        var command = new SignUpCommand
        {
            UserName = "admin",
            Email = "admin@test.com",
            Password = "password",
            Role = Roles.Admin.ToString()
        };
        var handler = new SignUpCommandHandler(_userManagerMock.Object);

        _userManagerMock.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable);


        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed creation" }));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.HasError.Should().BeTrue();
        result.Errors.Should().Contain("Failed creation");
    }

    [Fact]
    public async Task Handle_ValidAdminUser_CreatesUserSuccessfully()
    {
        // Arrange
        var command = new SignUpCommand
        {
            UserName = "admin",
            Name = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            Password = "password",
            Role = Roles.Admin.ToString(),
            IdentificationNumber = "01234567890",
            PhoneNumber = "809-111-2233"
        };

        var handler = new SignUpCommandHandler(_userManagerMock.Object);

        _userManagerMock.Setup(u => u.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userManagerMock.Setup(u => u.Users).Returns(new List<User>().AsQueryable());
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), command.Password)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), command.Role)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { Roles.Admin.ToString() });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.HasError.Should().BeFalse();
        result.Name.Should().Be("Admin");
        result.LastName.Should().Be("User");
        result.Email.Should().Be("admin@test.com");
        result.UserName.Should().Be("admin");
        result.Roles.Should().Contain(Roles.Admin.ToString());
    }
}
