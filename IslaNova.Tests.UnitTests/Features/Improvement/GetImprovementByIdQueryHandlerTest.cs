using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.Improvement.Queries.GetImprovementById;
using IslaNova.Core.Application.Mappings.DtosToEntities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Improvement
{
    public class GetImprovementByIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetImprovementByIdQueryHandlerTest()
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
        public async Task Handle_Should_Return_Improvement_When_Exists()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);
            var improvement = new Core.Domain.Entities.Feature.Improvement { ImprovementId = 1, Name = "Pool", Description = "Pool Description" };
            context.Improvements.Add(improvement);
            await context.SaveChangesAsync();

            var repository = new ImprovementRepository(context);
            var handler = new GetImprovementByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetImprovementByIdQuery() { ImprovementId = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.ImprovementId.Should().Be(improvement.ImprovementId);
            result.Name.Should().Be(improvement.Name);
            result.Description.Should().Be(improvement.Description);
        }

        [Fact]
        public async Task Handle_Should_Return_Null_When_Improvement_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new ImprovementRepository(context);
            var handler = new GetImprovementByIdQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetImprovementByIdQuery() { ImprovementId = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
