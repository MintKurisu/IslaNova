using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.Improvement.Commands.UpdateImprovement;
using IslaNova.Core.Application.Mappings.DtosToEntities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Improvement
{
    public class UpdateImprovementCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdateImprovementCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<ImprovementMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }


        [Fact]
        public async Task Handle_Should_Return_Dto_When_Improvement_Is_Updated()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var existingImprovement = new Core.Domain.Entities.Feature.Improvement
            {
                ImprovementId = 1,
                Name = "Pool",
                Description = "Pool Description"
            };
            context.Improvements.Add(existingImprovement);
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context);
            var handler = new UpdateImprovementCommandHandler(repository, _mapper);

            var command = new UpdateImprovementCommand
            {
                ImprovementId = 1,
                Name = "Pool",
                Description = "Pool Description"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var updated = await context.Improvements.FindAsync(command.ImprovementId);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be(command.Name);
            updated.Description.Should().Be(command.Description);
        }


        [Fact]
        public async Task Handler_Should_Fail_When_Improvement_Does_Not_Exits()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new ImprovementRepository(context);
            var handler = new UpdateImprovementCommandHandler(repository, _mapper);

            var command = new UpdateImprovementCommand
            {
                ImprovementId = 1,
                Name = "Pool",
                Description = "Pool Description"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .WithMessage("Improvement not found");
        }

    }
}
