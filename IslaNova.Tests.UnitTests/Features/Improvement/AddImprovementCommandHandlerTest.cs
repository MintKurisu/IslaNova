using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.Improvement.Commands.AddImprovement;
using IslaNova.Core.Application.Mappings.DtosToEntities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Improvement
{
    public class AddImprovementCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public AddImprovementCommandHandlerTest()
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
        public async Task Handle_Should_Return_Dto_When_Improvement_Is_Created()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var improvement = new Core.Domain.Entities.Feature.Improvement
            { ImprovementId = 1, Name = "Pool", Description = "Pool Description" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();


            var repository = new ImprovementRepository(context);
            var handler = new AddImprovementCommandHandler(repository, _mapper);

            var command = new AddImprovementCommand
            {
                Name = "Pool",
                Description = "A description for the improvement"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);


            // Assert
            result.Should().NotBeNull();

            var created = await context.Improvements.FindAsync(result.ImprovementId);
            created.Should().NotBeNull();
            created!.Name.Should().Be(command.Name);
            created.Description.Should().Be(command.Description);
        }


        [Fact]
        public void Validator_Should_Fail_When_Command_Is_Invalid()
        {
            var validator = new AddImprovementCommandValidator();

            var command = new AddImprovementCommand
            {
                Name = "",
                Description = ""
            };

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
        }


    }
}