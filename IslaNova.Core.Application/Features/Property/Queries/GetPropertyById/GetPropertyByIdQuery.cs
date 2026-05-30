using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertyById
{
    /// <summary>
    /// Query used to retrieve a single property by it's unique identfier
    /// </summary>
    public class GetPropertyByIdQuery : IRequest<PropertyApiDto?>
    {
        /// <example>5</example>
        [SwaggerParameter(Description = "Unique identifier of the property.")]
        public int PropertyId { get; set; }
    }

    public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyApiDto?>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetPropertyByIdQueryHandler(IPropertyRepository propertyRepository, IAuthServiceForWebApi authServiceForWebApi, IMapper mapper)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }


        public async Task<PropertyApiDto?> Handle(GetPropertyByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "PropertyImprovements.Improvement"])
                .FirstOrDefaultAsync(p => p.PropertyId == query.PropertyId);

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