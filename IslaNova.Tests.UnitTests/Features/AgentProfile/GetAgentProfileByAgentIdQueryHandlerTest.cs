using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.AgentProfile.Queries.GetAgentProfileByAgentId;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentProfile
{
    public class GetAgentProfileByAgentIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAgentProfileByAgentIdQueryHandlerTest()
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
        public async Task Handle_Should_Return_Profile_When_Exists()
        {
            using var context = new IslaNovaContext(_dbOptions);

            context.AgentProfiles.Add(new Core.Domain.Entities.AccountManagement.AgentProfile
            {
                AgentId = "agent001",
                Bio = "Agent bio",
                YearsOfExperience = 10
            });
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var handler = new GetAgentProfileByAgentIdQueryHandler(repository, _mapper);

            var result = await handler.Handle(new GetAgentProfileByAgentIdQuery { AgentId = "agent001" }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.AgentId.Should().Be("agent001");
            result.YearsOfExperience.Should().Be(10);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Profile_Not_Found()
        {
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new AgentProfileRepository(context);
            var handler = new GetAgentProfileByAgentIdQueryHandler(repository, _mapper);

            var result = await handler.Handle(new GetAgentProfileByAgentIdQuery { AgentId = "missing-agent" }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Validator_Should_Fail_When_Agent_Does_Not_Exist_In_Identity()
        {
            var authServiceMock = new Mock<IAuthServiceForWebApi>();
            authServiceMock.Setup(x => x.GetUserById("missing-agent"))
                .ReturnsAsync((Core.Application.Dtos.User.UserDto?)null);

            var validator = new GetAgentProfileByAgentIdQueryValidation(authServiceMock.Object);
            var query = new GetAgentProfileByAgentIdQuery { AgentId = "missing-agent" };

            var result = await validator.ValidateAsync(query);

            result.IsValid.Should().BeFalse();
        }
    }
}
