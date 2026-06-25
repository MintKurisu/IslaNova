using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.PropertyType.Commands.DeletePropertyType;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.PropertyType
{
    public class DeletePropertyTypeCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        public DeletePropertyTypeCommandHandlerTest()
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

            var propertyType = new Core.Domain.Entities.PropertyManagement.PropertyType
            {
                PropertyTypeId = 1,
                Name = "Apartment",
                Description = "Apartment Description"
            };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context);
            var handler = new DeletePropertyTypeCommandHandler(repository);

            var command = new DeletePropertyTypeCommand { PropertyTypeId = 1 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);


            // Assert
            result.Should().Be(MediatR.Unit.Value);

            var deleted = await context.Improvements.FindAsync(command.PropertyTypeId);
            deleted.Should().BeNull();
        }


        [Fact]
        public async Task Handle_Should_Throw_When_PropertyType_Does_Not_Exist()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new PropertyTypeRepository(context);
            var handler = new DeletePropertyTypeCommandHandler(repository);

            var command = new DeletePropertyTypeCommand { PropertyTypeId = 999 };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Error deleting property type");
        }
    }
}
