using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.PropertyType.Commands.UpdatePropertyType;
using IslaNova.Core.Application.Features.SaleType.Commands.UpdateSaleType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
namespace RealEstateApp.Tests.UnitTests.Features.SaleType
{
    public class UpdateSaleTypeCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateSaleTypeCommandHandlerTest()
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
        public async Task Handle_Should_Return_Dto_When_SaleType_Is_Updated()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var existingSaleType = new IslaNova.Core.Domain.Entities.PropertyManagement.SaleType
            {
                SaleTypeId = 1,
                Name = "For Rent",
                Description = "For Rent Description"
            };
            context.SaleTypes.Add(existingSaleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context);
            var handler = new UpdateSaleTypeCommandHandler(repository, _mapper);

            var command = new UpdateSaleTypeCommand
            {
                SaleTypeId = 1,
                Name = "For Rent",
                Description = "For Rent Description"
            };


            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var updated = await context.SaleTypes.FindAsync(command.SaleTypeId);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(command.Name);
            updated.Description.Should().Be(command.Description);
        }


        [Fact]
        public async Task Handler_Should_Fail_When_PropertyType_Does_Not_Exits()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyTypeRepository(context);
            var handler = new UpdatePropertyTypeCommandHandler(repository, _mapper);

            var command = new UpdatePropertyTypeCommand
            {
                PropertyTypeId = 1,
                Name = "Update Apartment",
                Description = "Update Apartment Description"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Property Type not found");
        }
    }
}
