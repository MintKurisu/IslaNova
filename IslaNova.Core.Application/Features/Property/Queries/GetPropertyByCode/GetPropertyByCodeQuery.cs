using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertyByCode
{
    /// <summary>
    /// Query used to retrieve a single property by it's unique Code
    /// </summary>
    public class GetPropertyByCodeQuery : IRequest<PropertyApiDto?>
    {
        /// <example>000001</example>
        [SwaggerParameter(Description = "Unique code of the property.")]
        public string? Code { get; set; }
    }

    public class GetPropertyByCodeQueryHandler : IRequestHandler<GetPropertyByCodeQuery, PropertyApiDto?>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        public GetPropertyByCodeQueryHandler(IPropertyRepository propertyRepository, IAuthServiceForWebApi authServiceForWebApi, IMapper mapper)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<PropertyApiDto?> Handle(GetPropertyByCodeQuery query, CancellationToken cancellationToken)
        {

            var entity = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.Code == query.Code);
            var dto = _mapper.Map<PropertyApiDto>(entity);

            if (entity != null)
            {
                var agent = await _authServiceForWebApi.GetUserById(entity.AgentId);

                if (agent != null)
                {
                    dto.AgentName = agent.Name + " " + agent.LastName;
                }
            }

            return dto;

        }
    }
}
