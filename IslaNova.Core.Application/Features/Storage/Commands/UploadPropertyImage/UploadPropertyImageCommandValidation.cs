using FluentValidation;

namespace IslaNova.Core.Application.Features.Storage.Commands.UploadPropertyImage
{
    public class UploadPropertyImageCommandValidation : AbstractValidator<UploadPropertyImageCommand>
    {
        private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxBytes = 10 * 1024 * 1024; 

        public UploadPropertyImageCommandValidation()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.File)
                .NotNull().WithMessage("A file is required.")
                .Must(f => f.Length > 0).WithMessage("The file cannot be empty.")
                .Must(f => f.Length <= MaxBytes).WithMessage("File size cannot exceed 10 MB.")
                .Must(f => AllowedTypes.Contains(f.ContentType)).WithMessage("Only JPEG, PNG, and WebP images are allowed.");
        }
    }
}
