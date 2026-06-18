using FluentValidation;

namespace IslaNova.Core.Application.Features.Property.Queries.GetAvailableProperties
{
    public class GetAvailablePropertiesValidation : AbstractValidator<GetAvailablePropertiesQuery>
    {
        public GetAvailablePropertiesValidation()
        {
            // No parameters to validate
        }
    }
}