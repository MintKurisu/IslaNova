using FluentValidation;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Commands.DeleteProperty
{
    public class DeletePropertyCommandValidation : AbstractValidator<DeletePropertyCommand>
    {
        private readonly IPropertyRepository _propertyRepository;

        public DeletePropertyCommandValidation(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified Property to delete does not exist.");
        }

        private async Task<bool> ExistProperty(int id, CancellationToken cancellationToken)
        {
            return await _propertyRepository.GetAllQuery().AnyAsync(p => p.PropertyId == id, cancellationToken);
        }
    }
}