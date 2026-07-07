using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using Xunit;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.PropertyManagement
{
    public class SaleTypeRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public SaleTypeRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_SaleType_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var saleType = new SaleType
            {
                Name = "Sale",
                Description = "Property for sale"
            };

            // Act
            var result = await repository.AddAsync(saleType);

            // Assert
            result.Should().NotBeNull();
            result!.SaleTypeId.Should().BeGreaterThan(0);
            result.Name.Should().Be("Sale");
            result.Description.Should().Be("Property for sale");

            var saleTypesInDb = await context.SaleTypes.ToListAsync();
            saleTypesInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<Exception>()
               .WithMessage("Value cannot be null. (Parameter 'entity')");

        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_SaleType_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var saleType = new SaleType
            {
                Name = "Rent",
                Description = "Property for rent"
            };
            saleType = await repository.AddAsync(saleType);

            // Act
            var result = await repository.GetByIdAsync(saleType!.SaleTypeId);

            // Assert
            result.Should().NotBeNull();
            result!.SaleTypeId.Should().Be(saleType.SaleTypeId);
            result.Name.Should().Be("Rent");
            result.Description.Should().Be("Property for rent");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_SaleType_In_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var saleType = new SaleType
            {
                Name = "Lease",
                Description = "Long term lease"
            };
            saleType = await repository.AddAsync(saleType);
            saleType!.Name = "Updated Lease";
            saleType.Description = "Updated long term lease";

            // Act
            var updated = await repository.UpdateAsync(saleType.SaleTypeId, saleType);

            // Assert
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Updated Lease");
            updated.Description.Should().Be("Updated long term lease");

            var fromDb = await repository.GetByIdAsync(saleType.SaleTypeId);
            fromDb!.Name.Should().Be("Updated Lease");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_SaleType_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var fakeSaleType = new SaleType
            {
                SaleTypeId = 9999,
                Name = "Fake",
                Description = "Does not exist"
            };

            // Act
            var updated = await repository.UpdateAsync(9999, fakeSaleType);

            // Assert
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_SaleType()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var saleType = new SaleType
            {
                Name = "Auction",
                Description = "Property auction"
            };
            saleType = await repository.AddAsync(saleType);

            // Act
            await repository.DeleteAsync(saleType!.SaleTypeId);

            // Assert
            var entity = await repository.GetByIdAsync(saleType.SaleTypeId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_SaleTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.SaleTypes.AddRange(
                new SaleType { Name = "Type1", Description = "Desc1" },
                new SaleType { Name = "Type2", Description = "Desc2" },
                new SaleType { Name = "Type3", Description = "Desc3" }
            );
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_SaleTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_SaleTypes()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new SaleTypeRepository(context);

            var saleTypes = new List<SaleType>
            {
                new SaleType { Name = "Sale", Description = "For sale" },
                new SaleType { Name = "Rent", Description = "For rent" },
                new SaleType { Name = "Lease", Description = "For lease" }
            };

            // Act
            var result = await repository.AddRangeAsync(saleTypes);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(st => st.SaleTypeId > 0);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            context.SaleTypes.AddRange(
                new SaleType { Name = "Sale", Description = "For sale" },
                new SaleType { Name = "Rent", Description = "For rent" }
            );
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = query.Where(st => st.Name.Contains("e")).ToList();

            // Assert
            result.Should().HaveCount(2); // Sale and Rent contain "e"
        }
    }
}
