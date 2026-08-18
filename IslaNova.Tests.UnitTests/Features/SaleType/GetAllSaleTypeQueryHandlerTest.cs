using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.SaleType.Queries.GetAllSaleType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace RealEstateApp.Tests.UnitTests.Features.SaleType
{
    public class GetAllSaleTypeQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllSaleTypeQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Sale_Type()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var saleType1 = new IslaNova.Core.Domain.Entities.PropertyManagement.SaleType { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" };
            var saleType2 = new IslaNova.Core.Domain.Entities.PropertyManagement.SaleType { SaleTypeId = 2, Name = "For Rent 2", Description = "For Rent 2 Description" };
            context.SaleTypes.AddRange(saleType1, saleType2);
            await context.SaveChangesAsync();


            var repository = new SaleTypeRepository(context);
            var handler = new GetAllSaleTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllSaleTypeQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data.All(i => i.Name is not null).Should().BeTrue();
            result.Data.First(i => i.Name == "For Rent");
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_SaleType_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new SaleTypeRepository(context);
            var handler = new GetAllSaleTypeQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllSaleTypeQuery(), CancellationToken.None);

            // Assert
            result.Data.Should().BeEmpty();
        }
    }
}
