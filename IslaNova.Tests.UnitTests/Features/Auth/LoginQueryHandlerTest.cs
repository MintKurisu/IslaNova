using FluentAssertions;
using IslaNova.Core.Domain.Settings;
using IslaNova.Infrastructure.Identity.Contexts;
using IslaNova.Infrastructure.Identity.Entities;
using IslaNova.Infrastructure.Identity.Features.Auth.Queries.Login;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace IslaNova.Tests.UnitTests.Features.Auth
{
    public class LoginQueryHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly DbContextOptions<IdentityContext> _dbContextOptions;

        public LoginQueryHandlerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            _jwtSettings = Options.Create(new JwtSettings
            {
                SecretKey = "VeryStrongSecretKeyForTestingPurposes123!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                DurationInMinutes = 60,
                RefreshTokenExpirationTime = 7
            });

            _dbContextOptions = new DbContextOptionsBuilder<IdentityContext>()
               .UseInMemoryDatabase($"IslaNovaTestDB_{Guid.NewGuid()}")
               .Options;
        }

        [Fact]
        public async Task Handle_ValidAdminUser_ReturnsJwtToken()
        {
            // Arrange
            var context = new IdentityContext(_dbContextOptions);

            var handler = new LoginQueryHandler(_userManagerMock.Object, _signInManagerMock.Object, _jwtSettings, context);
            var user = new User
            {
                Id = "001",
                UserName = "admin",
                Name = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                IdentificationNumber = "01234567890",
                EmailConfirmed = true,
            };

            var query = new LoginQuery { Identifier = "admin@test.com", Password = "pass" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user.UserName, query.Password, false, true))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
            _userManagerMock.Setup(u => u.GetClaimsAsync(user)).ReturnsAsync([]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HasError.Should().BeFalse();
            result.AccessToken.Should().NotBeNull();
            result.Name.Should().Be("Admin");
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsError()
        {
            // Arrange
            var context = new IdentityContext(_dbContextOptions);

            var handler = new LoginQueryHandler(_userManagerMock.Object, _signInManagerMock.Object, _jwtSettings, context);
            var query = new LoginQuery { Identifier = "unknown", Password = "pass" };

            _userManagerMock.Setup(u => u.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HasError.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain($"There's no account registered with this email: {query.Identifier ?? ""}");
        }
        [Fact]
        public async Task Handle_EmailNotConfirmed_ReturnsError()
        {
            // Arrange
            var context = new IdentityContext(_dbContextOptions);

            var handler = new LoginQueryHandler(_userManagerMock.Object, _signInManagerMock.Object, _jwtSettings, context);
            var user = new User
            {
                UserName = "john",
                Email = "john@example.com",
                EmailConfirmed = false,
                IdentificationNumber = "01234567890",
                Name = "Joe",
                LastName = "Doe"
            };
            var query = new LoginQuery { Identifier = "john@example.com", Password = "pass" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HasError.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.First(e => e == $"This account for user {query.Identifier} in not active, please check your email");
        }

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsError()
        {
            // Arrange
            var context = new IdentityContext(_dbContextOptions);

            var handler = new LoginQueryHandler(_userManagerMock.Object, _signInManagerMock.Object, _jwtSettings, context);
            var user = new User
            {
                UserName = "john",
                Email = "john@example.com",
                EmailConfirmed = true,
                IdentificationNumber = "01234567890",
                Name = "Joe",
                LastName = "Doe"
            };
            var query = new LoginQuery { Identifier = "john@example.com", Password = "wrongpass" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(user.UserName, query.Password, false, true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HasError.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors.Should().Contain($"This credentials are invalid for this email: {query.Identifier}");

        }

    }
}
