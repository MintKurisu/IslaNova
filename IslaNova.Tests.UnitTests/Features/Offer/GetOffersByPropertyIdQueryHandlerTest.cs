using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.Offer.Queries.GetOffersByPropertyId;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;
using IslaNova.Core.Application.Mappings.EntityToDtos.OfferManagement;

namespace IslaNova.Tests.UnitTests.Features.Offer
{
    public class GetOffersByPropertyIdQueryHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetOffersByPropertyIdQueryHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<OfferMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_All_Offers_For_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.Offers.AddRange(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "John Doe",
                    ContactPhone = "+1234567890",
                    Amount = 30000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 2,
                    PropertyId = 1,
                    ContactName = "Jane Smith",
                    ContactPhone = "+9876543210",
                    Amount = 31000.00m,
                    Status = OfferStatus.Accepted,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 3,
                    PropertyId = 2,
                    ContactName = "Bob Johnson",
                    ContactPhone = "+5555555555",
                    Amount = 32000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new GetOffersByPropertyIdQueryHandler(repository, _mapper);

            var query = new GetOffersByPropertyIdQuery
            {
                PropertyId = 1
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].ContactName.Should().Be("Jane Smith");
            result[1].ContactName.Should().Be("John Doe");
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Offers_For_Property()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new GetOffersByPropertyIdQueryHandler(repository, _mapper);

            var query = new GetOffersByPropertyIdQuery
            {
                PropertyId = 999
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_Return_Offers_Ordered_By_CreatedAt_Descending()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            var baseDate = DateTime.UtcNow;

            context.Offers.AddRange(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "First Offer",
                    ContactPhone = "+1111111111",
                    Amount = 30000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = baseDate
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 2,
                    PropertyId = 1,
                    ContactName = "Second Offer",
                    ContactPhone = "+2222222222",
                    Amount = 31000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = baseDate.AddDays(1)
                },
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 3,
                    PropertyId = 1,
                    ContactName = "Third Offer",
                    ContactPhone = "+3333333333",
                    Amount = 32000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = baseDate.AddDays(2)
                }
            );

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new GetOffersByPropertyIdQueryHandler(repository, _mapper);

            var query = new GetOffersByPropertyIdQuery
            {
                PropertyId = 1
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result[0].ContactName.Should().Be("Third Offer");
            result[1].ContactName.Should().Be("Second Offer");
            result[2].ContactName.Should().Be("First Offer");
        }
    }
}
