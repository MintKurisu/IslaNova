using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.SaleType.Queries.GetSaleTypeById;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.SaleType
{
    public class GetSaleTypeByIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetSaleTypeByIdQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<SaleTypeMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }


        [Fact]
        public async Task Handle_Should_Return_SaleType_When_Exists()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var saleType = new Core.Domain.Entities.PropertyManagement.SaleType { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context);
            var handler = new GetSaleTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetSaleTypeByIdQuery { SaleTypeId = 1 }, CancellationToken.None);


            // Assert
            result.Should().NotBeNull();
            result.SaleTypeId.Should().Be(saleType.SaleTypeId);
            result.Name.Should().Be(saleType.Name);
            result.Description.Should().Be(saleType.Description);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_PropertyType_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new SaleTypeRepository(context);
            var handler = new GetSaleTypeByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetSaleTypeByIdQuery() { SaleTypeId = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
