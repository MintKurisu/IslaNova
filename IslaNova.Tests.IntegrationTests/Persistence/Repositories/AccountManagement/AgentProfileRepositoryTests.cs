using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.AccountManagement
{
    public class AgentProfileRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;

        public AgentProfileRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_AgentProfile_To_Database()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            var profile = new AgentProfile
            {
                AgentId = "agent001",
                Bio = "Experienced agent",
                YearsOfExperience = 8
            };

            var result = await repository.AddAsync(profile);

            result.Should().NotBeNull();
            result!.AgentProfileId.Should().BeGreaterThan(0);

            var inDb = await context.AgentProfiles.ToListAsync();
            inDb.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_AgentProfile_When_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            var profile = await repository.AddAsync(new AgentProfile
            {
                AgentId = "agent001",
                Bio = "Bio"
            });

            var result = await repository.GetByIdAsync(profile!.AgentProfileId);

            result.Should().NotBeNull();
            result!.AgentId.Should().Be("agent001");
        }

        [Fact]
        public async Task GetByAgentIdAsync_Should_Return_AgentProfile_When_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            await repository.AddAsync(new AgentProfile
            {
                AgentId = "agent001",
                Bio = "Bio"
            });

            var result = await repository.GetByAgentIdAsync("agent001");

            result.Should().NotBeNull();
            result!.AgentId.Should().Be("agent001");
        }

        [Fact]
        public async Task GetByAgentIdAsync_Should_Return_Null_When_Not_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            var result = await repository.GetByAgentIdAsync("missing-agent");

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_AgentProfile_In_Database()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            var profile = await repository.AddAsync(new AgentProfile
            {
                AgentId = "agent001",
                Bio = "Old bio",
                YearsOfExperience = 3
            });

            profile!.Bio = "Updated bio";
            profile.YearsOfExperience = 10;

            var updated = await repository.UpdateAsync(profile.AgentProfileId, profile);

            updated.Should().NotBeNull();
            updated!.Bio.Should().Be("Updated bio");
            updated.YearsOfExperience.Should().Be(10);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_AgentProfile()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentProfileRepository(context);

            var profile = await repository.AddAsync(new AgentProfile
            {
                AgentId = "agent001",
                Bio = "Bio"
            });

            await repository.DeleteAsync(profile!.AgentProfileId);

            var entity = await repository.GetByIdAsync(profile.AgentProfileId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_AgentProfiles()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            context.AgentProfiles.AddRange(
                new AgentProfile { AgentId = "agent001", Bio = "Bio 1" },
                new AgentProfile { AgentId = "agent002", Bio = "Bio 2" }
            );
            await context.SaveChangesAsync();

            var repository = new AgentProfileRepository(context);
            var result = await repository.GetAllListAsync();

            result.Should().HaveCount(2);
        }
    }
}
