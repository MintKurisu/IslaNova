using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.AgentProfile.Queries.GetAllAgentProfiles;
using IslaNova.Core.Application.Mappings.EntityToDtos.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.UnitTests.Features.AgentProfile
{
    public class GetAllAgentProfilesQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllAgentProfilesQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Profiles()
        {
            using var context = new IslaNovaContext(_dbOptions);

            context.AgentProfiles.AddRange(
                new Core.Domain.Entities.AccountManagement.AgentProfile { AgentId = "agent001", Bio = "Bio 1" },
                new Core.Domain.Entities.AccountManagement.AgentProfile { AgentId = "agent002", Bio = "Bio 2" }
            );
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var handler = new GetAllAgentProfilesQueryHandler(repository, _mapper);

            var result = await handler.Handle(new GetAllAgentProfilesQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_When_No_Profiles()
        {
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new AgentProfileRepository(context);
            var handler = new GetAllAgentProfilesQueryHandler(repository, _mapper);

            var result = await handler.Handle(new GetAllAgentProfilesQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
