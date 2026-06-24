using FluentAssertions;
using IslaNova.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.SaleType.Commands.DeleteSaleType;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.SaleType
{
    public class DeleteSaleTypeCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public DeleteSaleTypeCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

        }

        [Fact]
        public async Task Handle_Should_Delete_PropertyType_When_It_Exists()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var saleType = new Core.Domain.Entities.PropertyManagement.SaleType
            {
                SaleTypeId = 1,
                Name = "For Rent",
                Description = "For Rent Description"
            };
            context.SaleTypes.Add(saleType);
            await context.SaveChangesAsync();

            var repository = new SaleTypeRepository(context);
            var handler = new DeleteSaleTypeCommandHandler(repository);

            var command = new DeleteSaleTypeCommand { SaleTypeId = 1 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(MediatR.Unit.Value);

            var deleted = await context.Improvements.FindAsync(command.SaleTypeId);
            deleted.Should().BeNull();
        }


        [Fact]
        public async Task Handle_Should_Throw_When_PropertyType_Does_Not_Exist()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new SaleTypeRepository(context);
            var handler = new DeleteSaleTypeCommandHandler(repository);

            var command = new DeleteSaleTypeCommand { SaleTypeId = 999 };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Error deleting sale type");
        }
    }
}
