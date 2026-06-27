using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IslaNova.Core.Application.Features.Property.Queries.FilterProperties;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Mappings.EntityToDtos;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.Property
{
    public class FilterPropertiesQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public FilterPropertiesQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<PropertyMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Filter_Properties_By_PropertyType()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.AddRange(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" },
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 2, Name = "House", Description = "House Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.AddRange(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 35000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Apartment 1",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 2,
                    SaleTypeId = 1,
                    Price = 65000.00m,
                    LandSize = 5000.0,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "House 1",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new FilterPropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new FilterPropertiesQuery
            {
                PropertyTypeId = 1
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PropertyId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_Should_Filter_Properties_By_Price_Range()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.AddRange(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 30000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 1",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 50000.00m,
                    LandSize = 3000.0,
                    Bedrooms = 5,
                    Bathrooms = 4,
                    Description = "Property 2",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 3,
                    Code = "000125",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 80000.00m,
                    LandSize = 4000.0,
                    Bedrooms = 6,
                    Bathrooms = 5,
                    Description = "Property 3",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new FilterPropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new FilterPropertiesQuery
            {
                MinPrice = 40000.00m,
                MaxPrice = 70000.00m
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PropertyId.Should().Be(2);
        }

        [Fact]
        public async Task Handle_Should_Filter_Properties_By_Bedrooms_And_Bathrooms()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.AddRange(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 30000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    Description = "Property 1",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 50000.00m,
                    LandSize = 3000.0,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Property 2",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new FilterPropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new FilterPropertiesQuery
            {
                Bedrooms = 4,
                Bathrooms = 3
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PropertyId.Should().Be(2);
        }

        [Fact]
        public async Task Handle_Should_Return_Only_Available_Properties()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.PropertyTypes.Add(
                new Core.Domain.Entities.PropertyManagement.PropertyType
                { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" }
            );

            context.SaleTypes.Add(
                new Core.Domain.Entities.PropertyManagement.SaleType
                { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" }
            );

            context.Properties.AddRange(
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 1,
                    Code = "000123",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 30000.00m,
                    LandSize = 2500.5,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Available Property",
                    AgentId = "agent001",
                    Status = PropertyStatus.Available,
                    CreatedAt = DateTime.UtcNow
                },
                new Core.Domain.Entities.PropertyManagement.Property
                {
                    PropertyId = 2,
                    Code = "000124",
                    PropertyTypeId = 1,
                    SaleTypeId = 1,
                    Price = 50000.00m,
                    LandSize = 3000.0,
                    Bedrooms = 4,
                    Bathrooms = 3,
                    Description = "Sold Property",
                    AgentId = "agent001",
                    Status = PropertyStatus.Sold,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new PropertyRepository(context);
            var authServiceMock = new Mock<IAuthServiceForWebApi>();

            var handler = new FilterPropertiesQueryHandler(repository, authServiceMock.Object, _mapper);

            var query = new FilterPropertiesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PropertyId.Should().Be(1);
        }
    }
}
