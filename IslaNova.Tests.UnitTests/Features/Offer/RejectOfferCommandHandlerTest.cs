using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Features.Offer.Commands.RejectOffer;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.UserInteraction;

namespace IslaNova.Tests.UnitTests.Features.Offer
{
    public class RejectOfferCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;

        public RejectOfferCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Reject_Pending_Offer()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.Offers.Add(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "John Doe",
                    ContactPhone = "+1234567890",
                    Amount = 30000.00m,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new RejectOfferCommandHandler(repository);

            var command = new RejectOfferCommand
            {
                OfferId = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            var rejectedOffer = await context.Offers.FindAsync(1);
            rejectedOffer?.Status.Should().Be(OfferStatus.Rejected);
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Offer_Not_Found()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            var repository = new OfferRepository(context);
            var handler = new RejectOfferCommandHandler(repository);

            var command = new RejectOfferCommand
            {
                OfferId = 999
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Should_Return_False_When_Offer_Not_Pending()
        {
            // Arrange
            using var context = new IslaNovaContext(_dbOptions);

            context.Offers.Add(
                new Core.Domain.Entities.UserInteraction.Offer
                {
                    OfferId = 1,
                    PropertyId = 1,
                    ContactName = "John Doe",
                    ContactPhone = "+1234567890",
                    Amount = 30000.00m,
                    Status = OfferStatus.Accepted,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var repository = new OfferRepository(context);
            var handler = new RejectOfferCommandHandler(repository);

            var command = new RejectOfferCommand
            {
                OfferId = 1
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}
