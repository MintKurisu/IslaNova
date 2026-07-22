using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.PropertyType.Queries.GetAllPropertyType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.PropertyType
{
    public class GetAllPropertyTypeQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllPropertyTypeQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Property_Type()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var propertyType1 = new Core.Domain.Entities.PropertyManagement.PropertyType { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" };
            var propertyType2 = new Core.Domain.Entities.PropertyManagement.PropertyType { PropertyTypeId = 2, Name = "Apartment 2", Description = "Apartment 2 Description" };
            context.PropertyTypes.AddRange(propertyType1, propertyType2);
            await context.SaveChangesAsync();


            var repository = new PropertyTypeRepository(context);
            var handler = new GetAllPropertyTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyTypeQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data.All(i => i.Name is not null).Should().BeTrue();
            result.Data.First(i => i.Name == "Apartment");
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_PropertyType_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyTypeRepository(context);
            var handler = new GetAllPropertyTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllPropertyTypeQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().BeEmpty();
        }
    }

}