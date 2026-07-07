using FluentAssertions;
using IslaNova.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.Improvement.Commands.DeleteImprovement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Improvement
{
    public class DeleteImprovementCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public DeleteImprovementCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

        }

        [Fact]
        public async Task Handle_Should_Delete_Improvement_When_It_Exists()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var improvement = new Core.Domain.Entities.Feature.Improvement
            { ImprovementId = 1, Name = "Pool", Description = "Pool Description" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();


            var repository = new ImprovementRepository(context);
            var handler = new DeleteImprovementCommandHandler(repository);

            var command = new DeleteImprovementCommand { ImprovementId = 1 };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);


            // Assert
            result.Should().Be(MediatR.Unit.Value);

            var deleted = await context.Improvements.FindAsync(command.ImprovementId);
            deleted.Should().BeNull();
        }


        [Fact]
        public async Task Handle_Should_Throw_When_Improvement_Does_Not_Exist()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new ImprovementRepository(context);
            var handler = new DeleteImprovementCommandHandler(repository);

            var command = new DeleteImprovementCommand { ImprovementId = 999 };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Error deleting improvement");
        }
    }
}
