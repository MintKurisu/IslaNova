using FluentValidation;
using IslaNova.Core.Domain.Interfaces.Feature;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Microsoft.EntityFrameworkCore;

namespace IslaNova.Core.Application.Features.Property.Commands.UpdateProperty
{
    public class UpdatePropertyCommandValidation : AbstractValidator<UpdatePropertyCommand>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IImprovementRepository _improvementRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB


        public UpdatePropertyCommandValidation(
            IPropertyRepository propertyRepository,
            IPropertyTypeRepository propertyTypeRepository,
            ISaleTypeRepository saleTypeRepository,
            IImprovementRepository improvementRepository,
            IPropertyImageRepository propertyImageRepository)
        {
            _propertyRepository = propertyRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _saleTypeRepository = saleTypeRepository;
            _improvementRepository = improvementRepository;
            _propertyImageRepository = propertyImageRepository;

            RuleFor(p => p.PropertyId)
                .GreaterThan(0).WithMessage("Property ID must be greater than 0.")
                .MustAsync(ExistProperty).WithMessage("The specified Property to update does not exist.");

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


            // --- Validación combinada del total de imágenes ---
            RuleFor(p => p)
                .Must(p => (p.ExistingImageUrls?.Count ?? 0) + (p.NewImagesFiles?.Count ?? 0) <= 10)
                .WithMessage("The property cannot have more than 10 images in total.")
                .OverridePropertyName("Images");

            RuleFor(p => p)
                .Must(p => (p.ExistingImageUrls?.Count ?? 0) + (p.NewImagesFiles?.Count ?? 0) >= 1)
                .WithMessage("The property must have at least 1 image.")
                .OverridePropertyName("Images");

            // --- Validación de archivos nuevos ---
            RuleForEach(p => p.NewImagesFiles)
                .Must(file => file.Length > 0)
                .WithMessage("Uploaded image file cannot be empty.")
                .Must(file => file.Length <= MaxImageSizeBytes)
                .WithMessage($"Each image must not exceed {MaxImageSizeBytes / (1024 * 1024)} MB.")
                .Must(file => AllowedImageExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage($"Only the following image formats are allowed: {string.Join(", ", AllowedImageExtensions)}.")
                .When(p => p.NewImagesFiles != null);

            // --- Validación de que las URLs existentes pertenezcan a la propiedad ---
            RuleFor(p => p.ExistingImageUrls)
                .MustAsync(async (command, urls, cancellation) =>
                    await BelongToProperty(command.PropertyId, urls, cancellation))
                .WithMessage("One or more existing image URLs do not belong to the specified property.")
                .When(p => p.ExistingImageUrls != null && p.ExistingImageUrls.Any());

            RuleFor(p => p.ImprovementIds)
                .MustAsync(ExistAllImprovements).WithMessage("One or more specified Improvement IDs do not exist in the database.");

            // --- Ubicación ---
            RuleFor(p => p.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.")
                .When(p => p.Latitude.HasValue);

            RuleFor(p => p.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.")
                .When(p => p.Longitude.HasValue);

            RuleFor(p => p)
                .Must(p => p.Latitude.HasValue == p.Longitude.HasValue)
                .WithMessage("Latitude and Longitude must be provided together.")
                .OverridePropertyName("Location");

            RuleFor(p => p.Address)
                .MaximumLength(300).WithMessage("Address must not exceed 300 characters.")
                .When(p => !string.IsNullOrEmpty(p.Address));

            RuleFor(p => p.City)
                .MaximumLength(150).WithMessage("City must not exceed 150 characters.")
                .When(p => !string.IsNullOrEmpty(p.City));

            RuleFor(p => p.NewImagesFiles)
                .Must(images => images == null || images.Count <= 10)
                .WithMessage("The property cannot have more than 10 images.");

            RuleFor(p => p.ExistingImageUrls)
               .Must(images => images == null || images.Count <= 10)
               .WithMessage("The property cannot have more than 10 images.");


            RuleFor(p => p.ImprovementIds)
                .MustAsync(ExistAllImprovements).WithMessage("One or more specified Improvement IDs do not exist in the database.");
        }

        private async Task<bool> ExistProperty(int id, CancellationToken cancellationToken)
        {
            return await _propertyRepository.GetAllQuery().AnyAsync(p => p.PropertyId == id, cancellationToken);
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

        private async Task<bool> BelongToProperty(int propertyId, List<string>? urls, CancellationToken cancellationToken)
        {
            if (urls == null || !urls.Any()) return true;

            var existingUrls = await _propertyImageRepository
                .GetAllQuery()
                .Where(img => img.PropertyId == propertyId)
                .Select(img => img.ImageUrl)
                .ToListAsync(cancellationToken);

            return urls.All(url => existingUrls.Contains(url));
        }
    }
}
