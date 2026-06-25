using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Entities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.Feature
{
    public class ImprovementRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public ImprovementRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Improvement_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);

            var improvement = new Improvement
            {
                Name = "Swimming Pool",
                Description = "Large swimming pool with heating"
            };

            // Act
            var result = await improvementRepository.AddAsync(improvement);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Swimming Pool");
            var assetTypes = await context.Improvements.ToListAsync();
            assetTypes.Should().ContainSingle();

        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);

            // Act
            Func<Task> act = async () => await improvementRepository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");

        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Improvement_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);
            var improvement = new Improvement
            {
                ImprovementId = 99,
                Name = "Swimming Pool",
                Description = "Large swimming pool with heating"
            };
           improvement = await improvementRepository.AddAsync(improvement);

            // Act
            var result = await improvementRepository.GetByIdAsync(improvement?.ImprovementId ?? 99);

            // Assert
            result.Should().NotBeNull();
            result!.ImprovementId.Should().Be(improvement?.ImprovementId);
            result.Name.Should().Be(improvement?.Name);
            result.Description.Should().Be(improvement?.Description);

        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);
     
            // Act
            var result = await improvementRepository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();

        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_Improvement_InDatabase()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);
            var improvement = new Improvement
            {
                Name = "Garage",
                Description = "Two car garage"
            };
            improvement = await improvementRepository.AddAsync(improvement);
            improvement!.Name = "Updated Garage";

            // Act
            var updated = await improvementRepository.UpdateAsync(improvement!.ImprovementId, improvement!);

            // Assert
           updated.Should().NotBeNull();
           updated!.Name.Should().Be("Updated Garage");


        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Improvement_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);
            var fakeImprovement = new Improvement
            {
                Name = "Garage",
                Description = "Two car garage"
            };

            // Act
            var updated = await improvementRepository.UpdateAsync(fakeImprovement!.ImprovementId, fakeImprovement!);

            // Assert
            updated.Should().BeNull();

        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Improvement()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);
            var improvement = new Improvement
            {
                Name = "Terrace",
                Description = "Rooftop terrace"
            };
            improvement = await improvementRepository.AddAsync(improvement);

            // Act
            await improvementRepository.DeleteAsync(improvement!.ImprovementId);
            var entity = await improvementRepository.GetByIdAsync(improvement!.ImprovementId);

            // Assert
            entity.Should().BeNull();


        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);

            // Act
            Func<Task> act = async () => await improvementRepository.DeleteAsync(999);

            // Assert
            await act.Should().NotThrowAsync();


        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Improvements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.AddRange(
                new Improvement { Name = "Pool", Description = "Swimming pool" },
                new Improvement { Name = "Gym", Description = "Fitness center" },
                new Improvement { Name = "Sauna", Description = "Finnish sauna" }
            );
            await context.SaveChangesAsync();
            var improvementRepository = new ImprovementRepository(context);

            // Act
            var result = await improvementRepository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_AssetTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var improvementRepository = new ImprovementRepository(context);

            // Act
            var result = await improvementRepository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllListWithInclude_Should_Include_PropertyImprovements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.Improvements.Add(new Improvement
            {
                Name = "Pool",
                Description = "Swimming pool",
                PropertyImprovements = new List<PropertyImprovement>
                {
                   new PropertyImprovement { PropertyId = 1 },
                   new PropertyImprovement { PropertyId = 2 }
                }
            });
            await context.SaveChangesAsync();
            var improvementRepository = new ImprovementRepository(context);

            // Act
            var result = await improvementRepository.GetAllListWithIncludeAsync(["PropertyImprovements"]);

            // Assert
            result.Should().NotBeEmpty();
            result[0].PropertyImprovements.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_Improvements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.Improvements.Add(new Improvement
            {
                Name = "Pool",
                Description = "Swimming pool",
            });
            await context.SaveChangesAsync();
            var improvementRepository = new ImprovementRepository(context);

            // Act
            var query = improvementRepository.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_PropertyImprovements()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.Improvements.Add(new Improvement
            {
                Name = "Pool",
                Description = "Swimming pool",
                PropertyImprovements = new List<PropertyImprovement>
                {
                   new PropertyImprovement { PropertyId = 1 },
                   new PropertyImprovement { PropertyId = 2 }
                }
            });
            await context.SaveChangesAsync();
            var improvementRepository = new ImprovementRepository(context);

            // Act
            var query = improvementRepository.GetAllQueryWithInclude(["PropertyImprovements"]);
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].PropertyImprovements.Should().NotBeEmpty();

        }
    }

}
