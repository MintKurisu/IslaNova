using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.PropertyManagement
{
    public class PropertyTypeRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public PropertyTypeRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyType_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var propertyType = new PropertyType
            {
                Name = "Apartment",
                Description = "Modern apartment"
            };

            // Act
            var result = await repository.AddAsync(propertyType);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyTypeId.Should().BeGreaterThan(0);
            result.Name.Should().Be("Apartment");
            result.Description.Should().Be("Modern apartment");

            var propertyTypesInDb = await context.PropertyTypes.ToListAsync();
            propertyTypesInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyType_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var propertyType = new PropertyType
            {
                Name = "House",
                Description = "Single family house"
            };
            propertyType = await repository.AddAsync(propertyType);

            // Act
            var result = await repository.GetByIdAsync(propertyType!.PropertyTypeId);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyTypeId.Should().Be(propertyType.PropertyTypeId);
            result.Name.Should().Be("House");
            result.Description.Should().Be("Single family house");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_PropertyType_In_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var propertyType = new PropertyType
            {
                Name = "Villa",
                Description = "Luxury villa"
            };
            propertyType = await repository.AddAsync(propertyType);
            propertyType!.Name = "Updated Villa";
            propertyType.Description = "Updated luxury villa";

            // Act
            var updated = await repository.UpdateAsync(propertyType.PropertyTypeId, propertyType);

            // Assert
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Villa");
            updated.Description.Should().Be("Updated luxury villa");

            var fromDb = await repository.GetByIdAsync(propertyType.PropertyTypeId);
            fromDb!.Name.Should().Be("Updated Villa");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_PropertyType_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var fakePropertyType = new PropertyType
            {
                PropertyTypeId = 9999,
                Name = "Fake",
                Description = "Does not exist"
            };

            // Act
            var updated = await repository.UpdateAsync(9999, fakePropertyType);

            // Assert
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyType()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var propertyType = new PropertyType
            {
                Name = "Condo",
                Description = "Condominium"
            };
            propertyType = await repository.AddAsync(propertyType);

            // Act
            await repository.DeleteAsync(propertyType!.PropertyTypeId);

            // Assert
            var entity = await repository.GetByIdAsync(propertyType.PropertyTypeId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_PropertyTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.PropertyTypes.AddRange(
                new PropertyType { Name = "Type1", Description = "Desc1" },
                new PropertyType { Name = "Type2", Description = "Desc2" },
                new PropertyType { Name = "Type3", Description = "Desc3" }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_PropertyTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_PropertyTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyTypeRepository(context);

            var propertyTypes = new List<PropertyType>
            {
                new PropertyType { Name = "Type1", Description = "Desc1" },
                new PropertyType { Name = "Type2", Description = "Desc2" },
                new PropertyType { Name = "Type3", Description = "Desc3" }
            };

            // Act
            var result = await repository.AddRangeAsync(propertyTypes);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(pt => pt.PropertyTypeId > 0);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.PropertyTypes.AddRange(
                new PropertyType { Name = "Sale", Description = "For sale" },
                new PropertyType { Name = "Rent", Description = "For rent" }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = query.Where(st => st.Name.Contains("e")).ToList();

            // Assert
            result.Should().HaveCount(2);
        }
    }

}
