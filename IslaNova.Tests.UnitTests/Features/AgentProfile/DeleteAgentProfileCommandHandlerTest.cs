using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Features.AgentProfile.Commands.DeleteAgentProfile;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentProfile
{
    public class DeleteAgentProfileCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public DeleteAgentProfileCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_Profile_When_Exists()
        {
            using var context = new IslaNovaContext(_dbOptions);

            context.AgentProfiles.Add(new Core.Domain.Entities.AccountManagement.AgentProfile
            {
                AgentProfileId = 1,
                AgentId = "agent001",
                Bio = "Profile"
            });
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var handler = new DeleteAgentProfileCommandHandler(repository);

            var result = await handler.Handle(new DeleteAgentProfileCommand { AgentId = "agent001" }, CancellationToken.None);

            result.Should().BeTrue();
            var deleted = await context.AgentProfiles.FirstOrDefaultAsync(x => x.AgentId == "agent001");
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Profile_Not_Found()
        {
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new AgentProfileRepository(context);
            var handler = new DeleteAgentProfileCommandHandler(repository);

            var result = await handler.Handle(new DeleteAgentProfileCommand { AgentId = "missing-agent" }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Validator_Should_Fail_When_Profile_Does_Not_Exist()
        {
            var repositoryMock = new Mock<IAgentProfileRepository>();
            repositoryMock.Setup(x => x.GetByAgentIdAsync("missing-agent"))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentProfile?)null);

            var validator = new DeleteAgentProfileCommandValidation(repositoryMock.Object);
            var command = new DeleteAgentProfileCommand { AgentId = "missing-agent" };

            var result = await validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
