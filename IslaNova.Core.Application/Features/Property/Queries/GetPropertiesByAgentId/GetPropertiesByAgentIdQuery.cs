using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Queries.GetPropertiesByAgentId
{
    public class GetPropertiesByAgentIdQuery : IRequest<IList<PropertyDto>>
    {
        [SwaggerParameter(Description = "Agent ID")]
        public string? AgentId { get; set; }
    }

    public class GetPropertiesByAgentIdQueryHandler : IRequestHandler<GetPropertiesByAgentIdQuery, IList<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetPropertiesByAgentIdQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<IList<PropertyDto>> Handle(GetPropertiesByAgentIdQuery query, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => p.AgentId == query.AgentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            var agent = await _authService.GetUserById(query.AgentId ?? "");
            var propertyDtos = new List<PropertyDto>();

            foreach (var property in properties)
            {
                var dto = _mapper.Map<PropertyDto>(property);

                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                }

                propertyDtos.Add(dto);
            }

            return propertyDtos;
        }
    }
}