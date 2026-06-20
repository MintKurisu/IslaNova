using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertyByCode
{
    public class GetPropertyByCodeQueryValidation : AbstractValidator<GetPropertyByCodeQuery>
    {
        public GetPropertyByCodeQueryValidation()
        {
            RuleFor(p => p.Code)
             .NotEmpty().WithMessage("Property code is required.")
             .WithMessage("Property code is required.");
        }
    }
}
