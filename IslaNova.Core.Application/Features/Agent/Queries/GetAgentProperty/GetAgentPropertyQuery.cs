using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Agent.Queries.GetAgentProperty
{
    /// <summary>
    /// Query used to retrieve all properties of an agente for it's unique identifier
    /// </summary>
    public class GetAgentPropertyQuery : IRequest<IList<PropertyApiDto>>
    {
        [SwaggerParameter(Description = "Unique identifier of the agent.")]
        public string? Id { get; set; }
    }

    public class GetAgentPropertyQueryHandler : IRequestHandler<GetAgentPropertyQuery, IList<PropertyApiDto>>
    {
        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        public GetAgentPropertyQueryHandler(IAuthServiceForWebApi authServiceForWebApi, IPropertyRepository propertyRepository, IMapper mapper)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<IList<PropertyApiDto>> Handle(GetAgentPropertyQuery query, CancellationToken cancellationToken)
        {
            var propertyList = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "PropertyImprovements.Improvement"])
                .Where(p => p.AgentId == query.Id)
                .ToListAsync();

            List<PropertyApiDto> dtoList = [];

            foreach (var property in propertyList)
            {
                var agent = await _authServiceForWebApi.GetUserById(property.AgentId);
                var dto = _mapper.Map<PropertyApiDto>(property);

                if (agent != null)
                {
                    dto.AgentName = agent.Name + " " + agent.LastName;
                }

                dtoList.Add(dto);
            }

            return dtoList;

        }
    }
}