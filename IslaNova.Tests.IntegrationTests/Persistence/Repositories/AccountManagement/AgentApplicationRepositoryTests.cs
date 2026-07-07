using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Entities.AccountManagement;
using IslaNova.Core.Domain.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.AccountManagement;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.AccountManagement
{
    public class AgentApplicationRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;

        public AgentApplicationRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        private AgentApplication BuildApplication(string userId = "user-001") => new()
        {
            UserId = userId,
            ProfessionalStatement = "I have 5 years of experience in real estate.",
            EmploymentType = EmploymentType.Independent,
            Status = ApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        // ─── AddAsync ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_Should_Add_AgentApplication_To_Database()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var application = BuildApplication();

            var result = await repository.AddAsync(application);

            result.Should().NotBeNull();
            result!.ApplicationId.Should().BeGreaterThan(0);

            var inDb = await context.AgentApplications.ToListAsync();
            inDb.Should().ContainSingle();
        }

        // ─── GetByIdAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Application_When_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var added = await repository.AddAsync(BuildApplication("user-002"));

            var result = await repository.GetByIdAsync(added!.ApplicationId);

            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-002");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var result = await repository.GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // ─── GetByUserIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Application_When_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            await repository.AddAsync(BuildApplication("user-003"));

            var result = await repository.GetByUserIdAsync("user-003");

            result.Should().NotBeNull();
            result!.UserId.Should().Be("user-003");
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Null_When_Not_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var result = await repository.GetByUserIdAsync("nonexistent-user");

            result.Should().BeNull();
        }

        // ─── ExistsByUserIdAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task ExistsByUserIdAsync_Should_Return_True_When_Application_Exists()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            await repository.AddAsync(BuildApplication("user-004"));

            var exists = await repository.ExistsByUserIdAsync("user-004");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByUserIdAsync_Should_Return_False_When_Application_Does_Not_Exist()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var exists = await repository.ExistsByUserIdAsync("user-nobody");

            exists.Should().BeFalse();
        }

        // ─── UpdateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Modify_Status_And_ReviewFields()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var added = await repository.AddAsync(BuildApplication("user-005"));

            added!.Status = ApplicationStatus.Approved;
            added.ReviewedBy = "admin-001";
            added.ReviewedAt = DateTime.UtcNow;
            added.AdminComments = "Looks good!";

            var updated = await repository.UpdateAsync(added.ApplicationId, added);

            updated.Should().NotBeNull();
            updated!.Status.Should().Be(ApplicationStatus.Approved);
            updated.ReviewedBy.Should().Be("admin-001");
            updated.AdminComments.Should().Be("Looks good!");
            updated.ReviewedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Application_Does_Not_Exist()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var fakeApplication = BuildApplication("user-ghost");
            fakeApplication.ApplicationId = 9999;

            var result = await repository.UpdateAsync(9999, fakeApplication);

            result.Should().BeNull();
        }

        // ─── DeleteAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_Should_Remove_Application()
        {
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new AgentApplicationRepository(context);

            var added = await repository.AddAsync(BuildApplication("user-006"));

            await repository.DeleteAsync(added!.ApplicationId);

            var entity = await repository.GetByIdAsync(added.ApplicationId);
            entity.Should().BeNull();
        }

        // ─── GetAllListAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Applications()
        {
            using var context = new IslaNovaContext(_dbContextOptions);

            context.AgentApplications.AddRange(
                new AgentApplication
                {
                    UserId = "user-007",
                    ProfessionalStatement = "Statement A",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Pending
                },
                new AgentApplication
                {
                    UserId = "user-008",
                    ProfessionalStatement = "Statement B",
                    EmploymentType = EmploymentType.Agency,
                    Status = ApplicationStatus.Approved
                }
            );
            await context.SaveChangesAsync();

            var repository = new AgentApplicationRepository(context);
            var result = await repository.GetAllListAsync();

            result.Should().HaveCount(2);
        }

        // ─── GetAllQuery ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_With_All_Applications()
        {
            using var context = new IslaNovaContext(_dbContextOptions);

            context.AgentApplications.AddRange(
                new AgentApplication
                {
                    UserId = "user-009",
                    ProfessionalStatement = "Statement C",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Rejected
                },
                new AgentApplication
                {
                    UserId = "user-010",
                    ProfessionalStatement = "Statement D",
                    EmploymentType = EmploymentType.Independent,
                    Status = ApplicationStatus.Pending
                }
            );
            await context.SaveChangesAsync();

            var repository = new AgentApplicationRepository(context);
            var query = repository.GetAllQuery();

            var pending = await query
                .Where(a => a.Status == ApplicationStatus.Pending)
                .ToListAsync();

            pending.Should().HaveCount(1);
            pending.First().UserId.Should().Be("user-010");
        }
    }
}
