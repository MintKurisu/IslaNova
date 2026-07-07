using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IslaNova.Core.Application.Features.SaleType.Commands.AddSaleType;
using IslaNova.Core.Application.Mappings.DtosToEntities.PropertyManagement;
using IslaNova.Infrastructure.Persistence.Contexts;
using IslaNova.Infrastructure.Persistence.Repositories.PropertyManagement;

namespace IslaNova.Tests.UnitTests.Features.SaleType
{
    public class AddSaleTypeCommandHandlerTest
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
                configExpression.AddProfile<SaleTypeMappingProfile>();

                var config = new MapperConfiguration(configExpression, loggerFactory);
                _mapper = config.CreateMapper();
            }


            [Fact]
            public async Task Handle_Should_Return_Dto_When_SaleType_Is_Created()
            {
                // arrange
                using var context = new IslaNovaContext(_dbOptions);

                var saleType = new Core.Domain.Entities.PropertyManagement.SaleType { SaleTypeId = 1, Name = "For Rent", Description = "For Rent Description" };
                context.SaleTypes.Add(saleType);
                await context.SaveChangesAsync();

                var repository = new SaleTypeRepository(context);
                var handler = new AddSaleTypeCommandHandler(repository, _mapper);

                var command = new AddSaleTypeCommand
                {
                    Name = "For Rent",
                    Description = "For Rent Description"
                };

                // Act
                var result = await handler.Handle(command, CancellationToken.None);


                // Assert
                result.Should().NotBeNull();

                var created = await context.SaleTypes.FindAsync(result.SaleTypeId);
                created.Should().NotBeNull();
                created!.Name.Should().Be(command.Name);
                created.Description.Should().Be(command.Description);
            }


            [Fact]
            public void Handler_Validator_Should_Fail_When_Command_Is_Invalid()
            {
                var validator = new AddSaleTypeCommandValidation();

                var command = new AddSaleTypeCommand
                {
                    Name = "",
                    Description = ""
                };

                var result = validator.Validate(command);

                result.IsValid.Should().BeFalse();
            }
        }
    }
}