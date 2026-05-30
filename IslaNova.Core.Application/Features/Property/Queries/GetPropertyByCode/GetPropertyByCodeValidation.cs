using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertyByCode
{
    public class GetPropertyByCodeValidation : AbstractValidator<GetPropertyByCodeQuery>
    {
        public GetPropertyByCodeValidation()
        {
            RuleFor(p => p.Code)
             .NotEmpty().WithMessage("Property code is required.")
             .WithMessage("Property code is required.");
        }
    }
}
