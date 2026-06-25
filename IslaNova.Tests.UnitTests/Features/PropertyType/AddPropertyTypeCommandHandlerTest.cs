using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.PropertyType.Commands.AddPropertyType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;
using static IslaNova.Core.Application.Features.PropertyType.Commands.AddPropertyType.AddPropertyTypeCommand;

namespace IslaNova.Tests.UnitTests.Features.PropertyType
{
    public class AddPropertyTypeCommandHandlerTest
    {
        private readonly DbContextOptions<IslaNovaContext> _dbOptions;
        private readonly IMapper _mapper;

        public AddPropertyTypeCommandHandlerTest()
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
        public async Task Handle_Should_Return_Dto_When_PropertyType_Is_Created()
        {
            // arrange
            using var context = new IslaNovaContext(_dbOptions);

            var propertyType = new Core.Domain.Entities.PropertyManagement.PropertyType { PropertyTypeId = 1, Name = "Apartment", Description = "Apartment Description" };
            context.PropertyTypes.Add(propertyType);
            await context.SaveChangesAsync();


            var repository = new PropertyTypeRepository(context);
            var handler = new AddPropertyTypeCommandHandler(repository, _mapper);

            var command = new AddPropertyTypeCommand
            {
                Name = "Apartment",
                Description = "Apartment Description"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);


            // Assert
            result.Should().NotBeNull();

            var created = await context.PropertyTypes.FindAsync(result.PropertyTypeId);
            created.Should().NotBeNull();
            created!.Name.Should().Be(command.Name);
            created.Description.Should().Be(command.Description);
        }


        [Fact]
        public void Handler_Validator_Should_Fail_When_Command_Is_Invalid()
        {
            var validator = new AddPropertyTypeCommandValidation();

            var command = new AddPropertyTypeCommand
            {
                Name = "",
                Description = ""
            };

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
        }
    }
}
