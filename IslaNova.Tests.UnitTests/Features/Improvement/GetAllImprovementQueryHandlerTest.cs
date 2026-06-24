using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.Improvement.Queries.GetAllImprovement;
using IslaNova.Core.Application.Mappings.DtosToEntities.Feature;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.Feature;

namespace IslaNova.Tests.UnitTests.Features.Improvement
{
    public class GetAllImprovementQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllImprovementQueryHandlerTest()
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
        public async Task Handle_Should_Return_All_Improvements()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var improvement1 = new Core.Domain.Entities.Feature.Improvement { ImprovementId = 1, Name = "Pool", Description = "Pool Description" };
            var improvement2 = new Core.Domain.Entities.Feature.Improvement { ImprovementId = 2, Name = "Wi-Fi", Description = "Wi-Fi Description" };
            context.Improvements.AddRange(improvement1, improvement2);
            await context.SaveChangesAsync();


            var repository = new ImprovementRepository(context);
            var handler = new GetAllImprovementQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllImprovementQuery(), CancellationToken.None);


            // Assert
            result.Should().HaveCount(2);
            result.All(i => i.Name is not null).Should().BeTrue();
            result.First(i => i.Name == "Pool");
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Improvement_Exists()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);
            var repository = new ImprovementRepository(context);
            var handler = new GetAllImprovementQueryHandler(repository, _mapper);

            // Act
            var result = await handler.Handle(new GetAllImprovementQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
