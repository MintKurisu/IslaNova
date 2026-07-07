using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetAllProperty
{
    public class GetAllPropertyQueryValidation : AbstractValidator<GetAllPropertyQuery>
    {
        public GetAllPropertyQueryValidation()
        {
            // No parameters to validate
        }
    }
}
