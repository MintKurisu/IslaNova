using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Exceptions;
using IslaNova.Core.Application.Features.PropertyType.Commands.UpdatePropertyType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.PropertyType
{
    public class UpdatePropertyTypeCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public UpdatePropertyTypeCommandHandlerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<IslaNovaContext>()
                .UseInMemoryDatabase(databaseName: $"Db_{Guid.NewGuid()}")
                .Options;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var configExpression = new MapperConfigurationExpression();
            configExpression.AddProfile<PropertyTypeMappingProfile>();

            var config = new MapperConfiguration(configExpression, loggerFactory);
            _mapper = config.CreateMapper();
        }


        [Fact]
        public async Task Handle_Should_Return_Dto_When_PropertyType_Is_Updated()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);


            var existingPropertyType = new Core.Domain.Entities.PropertyManagement.PropertyType
            {
                PropertyTypeId = 1,
                Name = "Apartment",
                Description = "Apartment Description"
            };
            context.PropertyTypes.Add(existingPropertyType);
            await context.SaveChangesAsync();

            var repository = new PropertyTypeRepository(context);
            var handler = new UpdatePropertyTypeCommandHandler(repository, _mapper);

            var command = new UpdatePropertyTypeCommand
            {
                PropertyTypeId = 1,
                Name = "Update Apartment",
                Description = "Update Apartment Description"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var updated = await context.PropertyTypes.FindAsync(command.PropertyTypeId);
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
