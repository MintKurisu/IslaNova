using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Features.Property.Queries.GetAvailableProperties
{
    public class GetAvailablePropertiesQuery : IRequest<IList<PropertyDto>> { }

    public class GetAvailablePropertiesQueryHandler : IRequestHandler<GetAvailablePropertiesQuery, IList<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetAvailablePropertiesQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<IList<PropertyDto>> Handle(GetAvailablePropertiesQuery query, CancellationToken cancellationToken)
        {
            var properties = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => p.Status == PropertyStatus.Available)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);

            var propertyDtos = new List<PropertyDto>();

            foreach (var property in properties)
            {
                var agent = await _authService.GetUserById(property.AgentId);
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