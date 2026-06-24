using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.PropertyType.Queries.GetPropertyTypeById;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.PropertyType
{
    public class GetPropertyTypeByIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetPropertyTypeByIdQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<PropertyTypeMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }


        [Fact]
        public async Task Handle_Should_Return_PropertyType_When_Exists()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var propertyType = new Core.Domain.Entities.PropertyManagement.PropertyType { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context);
            var handler = new GetPropertyTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetPropertyTypeByIdQuery { PropertyTypeId = 1 }, CancellationToken.None);


            // Assert
            result.Should().NotBeNull();
            result.PropertyTypeId.Should().Be(propertyType.PropertyTypeId);
            result.Name.Should().Be(propertyType.Name);
            result.Description.Should().Be(propertyType.Description);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_PropertyType_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyTypeRepository(context);
            var handler = new GetPropertyTypeByIdQueryHandler(repository, _mapper);


            // Act
            var result = await handler.Handle(new GetPropertyTypeByIdQuery() { PropertyTypeId = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
