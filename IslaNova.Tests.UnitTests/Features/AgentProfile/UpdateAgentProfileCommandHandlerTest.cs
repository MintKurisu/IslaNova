using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.AgentProfile.Commands.UpdateAgentProfile;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentProfile
{
    public class UpdateAgentProfileCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateAgentProfileCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<AgentProfileMappingProfile>();
            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Update_Profile_When_Exists()
        {
            using var context = new IslaNovaContext(_dbOptions);

            context.AgentProfiles.Add(new Core.Domain.Entities.AccountManagement.AgentProfile
            {
                AgentProfileId = 1,
                AgentId = "agent001",
                Bio = "Old bio",
                YearsOfExperience = 2
            });
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var handler = new UpdateAgentProfileCommandHandler(repository, _mapper);

            var command = new UpdateAgentProfileCommand
            {
                AgentId = "agent001",
                Bio = "Updated bio",
                YearsOfExperience = 7,
                SpecialtyZones = "Distrito Nacional"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Bio.Should().Be("Updated bio");
            result.YearsOfExperience.Should().Be(7);

            var updated = await context.AgentProfiles.FirstAsync(x => x.AgentId == "agent001");
            updated.Bio.Should().Be("Updated bio");
            updated.SpecialtyZones.Should().Be("Distrito Nacional");
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Profile_Not_Found()
        {
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new AgentProfileRepository(context);
            var handler = new UpdateAgentProfileCommandHandler(repository, _mapper);

            var command = new UpdateAgentProfileCommand
            {
                AgentId = "agent-does-not-exist",
                Bio = "Updated bio"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Validator_Should_Fail_When_Profile_Does_Not_Exist()
        {
            var repositoryMock = new Mock<IAgentProfileRepository>();
            repositoryMock.Setup(x => x.GetByAgentIdAsync("missing-agent"))
                .ReturnsAsync((Core.Domain.Entities.AccountManagement.AgentProfile?)null);

            var validator = new UpdateAgentProfileCommandValidation(repositoryMock.Object);
            var command = new UpdateAgentProfileCommand { AgentId = "missing-agent" };

            var result = await validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
