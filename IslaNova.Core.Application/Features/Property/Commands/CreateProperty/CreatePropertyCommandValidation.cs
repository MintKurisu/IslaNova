using FluentValidation;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Commands.CreateProperty
{
    public class CreatePropertyCommandValidation : AbstractValidator<CreatePropertyCommand>
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IImprovementRepository _improvementRepository;

        public CreatePropertyCommandValidation(
            IPropertyTypeRepository propertyTypeRepository,
            ISaleTypeRepository saleTypeRepository,
            IImprovementRepository improvementRepository)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _saleTypeRepository = saleTypeRepository;
            _improvementRepository = improvementRepository;

            RuleFor(p => p.PropertyTypeId)
                .GreaterThan(0).WithMessage("Property type ID must be greater than 0.")
                .MustAsync(ExistPropertyType).WithMessage("The specified Property Type does not exist.");

            RuleFor(p => p.SaleTypeId)
                .GreaterThan(0).WithMessage("Sale type ID must be greater than 0.")
                .MustAsync(ExistSaleType).WithMessage("The specified Sale Type does not exist.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be a positive value greater than 0.");

            RuleFor(p => p.LandSize)
                .GreaterThan(0).WithMessage("Land size must be greater than 0 square meters.");

            RuleFor(p => p.Bedrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Bedrooms count cannot be negative.");

            RuleFor(p => p.Bathrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Bathrooms count cannot be negative.");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("Property description is required.")
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

            RuleFor(p => p.AgentId)
                .NotEmpty().WithMessage("Agent ID is required.");

            RuleFor(p => p.ImageUrls)
                .Must(images => images == null || images.Count <= 10)
                .WithMessage("The property cannot have more than 10 images.");

            RuleForEach(p => p.ImageUrls)
                .NotEmpty().WithMessage("Image URL cannot be empty.");

            RuleFor(p => p.ImprovementIds)
                .MustAsync(ExistAllImprovements).WithMessage("One or more specified Improvement IDs do not exist in the database.");
        }

        private async Task<bool> ExistPropertyType(int id, CancellationToken cancellationToken)
        {
            return await _propertyTypeRepository.GetAllQuery().AnyAsync(pt => pt.PropertyTypeId == id, cancellationToken);
        }

        private async Task<bool> ExistSaleType(int id, CancellationToken cancellationToken)
        {
            return await _saleTypeRepository.GetAllQuery().AnyAsync(st => st.SaleTypeId == id, cancellationToken);
        }

        private async Task<bool> ExistAllImprovements(List<int>? improvementIds, CancellationToken cancellationToken)
        {
            if (improvementIds == null || !improvementIds.Any())
                return true;

            var existingCount = await _improvementRepository.GetAllQuery()
                .Where(i => improvementIds.Contains(i.ImprovementId))
                .CountAsync(cancellationToken);

            return existingCount == improvementIds.Distinct().Count();
        }
    }
}
