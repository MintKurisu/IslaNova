using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.AgentProfile.Commands.CreateAgentProfile;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Core.Domain.Interfaces.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentProfile
{
    public class CreateAgentProfileCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreateAgentProfileCommandHandlerTest()
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
        public async Task Handle_Should_Create_Profile_When_Not_Exists()
        {
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new AgentProfileRepository(context);
            var handler = new CreateAgentProfileCommandHandler(repository, _mapper);

            var command = new CreateAgentProfileCommand
            {
                AgentId = "agent001",
                Bio = "Real estate agent",
                YearsOfExperience = 5,
                WhatsappNumber = "+18095550001",
                FacebookUrl = "https://facebook.com/agent001",
                InstagramUrl = "https://instagram.com/agent001",
                SpecialtyZones = "Santo Domingo"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result!.AgentId.Should().Be("agent001");
            result.YearsOfExperience.Should().Be(5);

            var created = await context.AgentProfiles.FirstOrDefaultAsync(x => x.AgentId == "agent001");
            created.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Profile_Already_Exists()
        {
            using var context = new IslaNovaContext(_dbOptions);

            context.AgentProfiles.Add(new Core.Domain.Entities.AccountManagement.AgentProfile
            {
                AgentId = "agent001",
                Bio = "Existing profile"
            });
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var handler = new CreateAgentProfileCommandHandler(repository, _mapper);

            var command = new CreateAgentProfileCommand
            {
                AgentId = "agent001",
                Bio = "Another profile"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Validator_Should_Fail_When_AgentId_Is_Empty()
        {
            var repositoryMock = new Mock<IAgentProfileRepository>();
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var validator = new CreateAgentProfileCommandValidation(repositoryMock.Object, authServiceMock.Object);
            var command = new CreateAgentProfileCommand { AgentId = "" };

            var result = await validator.ValidateAsync(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
