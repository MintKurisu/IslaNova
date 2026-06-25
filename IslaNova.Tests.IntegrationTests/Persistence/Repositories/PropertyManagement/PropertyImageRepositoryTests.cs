using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using Xunit;

namespace IslaNova.Tests.IntegrationTests.Persistence.Repositories.PropertyManagement
{
    public class PropertyImageRepositoryTests
    {
        private readonly DbContextOptions<IslaNovaContext> _dbContextOptions;
        public PropertyImageRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"IslaNovaDb_{Guid.NewGuid()}")
                .Options;
        }

        private async Task<Property> CreateTestProperty(IslaNovaContext context)
        {
            var propertyType = new PropertyType { Name = "House", Description = "Test" };
            var saleType = new SaleType { Name = "Sale", Description = "Test" };
            context.PropertyTypes.Add(propertyType);
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var property = new Property
            {
                Code = $"PROP{Guid.NewGuid().ToString().Substring(0, 6)}",
                PropertyTypeId = propertyType.PropertyTypeId,
                SaleTypeId = saleType.SaleTypeId,
                Price = 250000m,
                LandSize = 500,
                Bedrooms = 3,
                Bathrooms = 2,
                Description = "Test property",
                AgentId = "agent123",
                Status = PropertyStatus.Available
            };
            context.Properties.Add(property);
            await context.SaveChangesAsync();

            return property;
        }

        [Fact]
        public async Task AddAsync_Should_Add_PropertyImage_To_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImage = new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/images/property1.jpg"
            };

            // Act
            var result = await repository.AddAsync(propertyImage);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyImageId.Should().BeGreaterThan(0);
            result.ImageUrl.Should().Be("/images/property1.jpg");
            result.PropertyId.Should().Be(property.PropertyId);

            var imagesInDb = await context.PropertyImages.ToListAsync();
            imagesInDb.Should().ContainSingle();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImageRepository(context);

            // Act
            Func<Task> act = async () => await repository.AddAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'entity')");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_PropertyImage_When_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImage = new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/images/property2.jpg"
            };
            propertyImage = await repository.AddAsync(propertyImage);

            // Act
            var result = await repository.GetByIdAsync(propertyImage!.PropertyImageId);

            // Assert
            result.Should().NotBeNull();
            result!.PropertyImageId.Should().Be(propertyImage.PropertyImageId);
            result.ImageUrl.Should().Be("/images/property2.jpg");
            result.PropertyId.Should().Be(property.PropertyId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImageRepository(context);

            // Act
            var result = await repository.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Modify_PropertyImage_In_Database()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImage = new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/images/old.jpg"
            };
            propertyImage = await repository.AddAsync(propertyImage);
            propertyImage!.ImageUrl = "/images/new.jpg";

            // Act
            var updated = await repository.UpdateAsync(propertyImage.PropertyImageId, propertyImage);

            // Assert
            updated.Should().NotBeNull();
            updated!.ImageUrl.Should().Be("/images/new.jpg");

            var fromDb = await repository.GetByIdAsync(propertyImage.PropertyImageId);
            fromDb!.ImageUrl.Should().Be("/images/new.jpg");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_PropertyImage_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImageRepository(context);

            var fakeImage = new PropertyImage
            {
                PropertyImageId = 9999,
                PropertyId = 1,
                ImageUrl = "/fake.jpg"
            };

            // Act
            var updated = await repository.UpdateAsync(9999, fakeImage);

            // Assert
            updated.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_PropertyImage()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImage = new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/images/todelete.jpg"
            };
            propertyImage = await repository.AddAsync(propertyImage);

            // Act
            await repository.DeleteAsync(propertyImage!.PropertyImageId);

            // Assert
            var entity = await repository.GetByIdAsync(propertyImage.PropertyImageId);
            entity.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImageRepository(context);

            // Act
            Func<Task> act = async () => await repository.DeleteAsync(9999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_PropertyImages()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.PropertyImages.AddRange(
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img1.jpg" },
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img2.jpg" },
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img3.jpg" }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyImageRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_PropertyImages()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var repository = new PropertyImageRepository(context);

            // Act
            var result = await repository.GetAllListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithIncludeAsync_Should_Include_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImage = new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/images/withprop.jpg"
            };
            propertyImage = await repository.AddAsync(propertyImage);

            // Act
            var result = await repository.GetByIdWithIncludeAsync(
                propertyImage!.PropertyImageId,
                new List<string> { "Property" }
            );

            // Assert
            result.Should().NotBeNull();
            result!.Property.Should().NotBeNull();
            result.Property!.PropertyId.Should().Be(property.PropertyId);
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.PropertyImages.AddRange(
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/image1.jpg" },
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/image2.jpg" }
            );
            await context.SaveChangesAsync();

            var repository = new PropertyImageRepository(context);

            // Act
            var query = repository.GetAllQuery();
            var result = await query.ToListAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Include_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);

            context.PropertyImages.Add(new PropertyImage
            {
                PropertyId = property.PropertyId,
                ImageUrl = "/test.jpg"
            });
            await context.SaveChangesAsync();

            var repository = new PropertyImageRepository(context);

            // Act
            var query = repository.GetAllQueryWithInclude(new List<string> { "Property" });
            var result = await query.ToListAsync();

            // Assert
            result.Should().NotBeEmpty();
            result[0].Property.Should().NotBeNull();
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_PropertyImages()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbContextOptions);
            var property = await CreateTestProperty(context);
            var repository = new PropertyImageRepository(context);

            var propertyImages = new List<PropertyImage>
            {
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img1.jpg" },
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img2.jpg" },
                new PropertyImage { PropertyId = property.PropertyId, ImageUrl = "/img3.jpg" }
            };

            // Act
            var result = await repository.AddRangeAsync(propertyImages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(pi => pi.PropertyImageId > 0);
        }
    }
}
