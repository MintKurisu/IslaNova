using AutoMapper;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.UserInteraction;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Favorite.Queries.GetClientFavoriteProperties
{
    public class GetClientFavoritePropertiesQuery : IRequest<IList<PropertyDto>>
    {
        [SwaggerParameter(Description = "Client ID")]
        public string? ClientId { get; set; }
    }

    public class GetClientFavoritePropertiesQueryHandler : IRequestHandler<GetClientFavoritePropertiesQuery, IList<PropertyDto>>
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public GetClientFavoritePropertiesQueryHandler(
            IFavoriteRepository favoriteRepository,
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<IList<PropertyDto>> Handle(GetClientFavoritePropertiesQuery query, CancellationToken cancellationToken)
        {
            var favoritePropertyIds = await _favoriteRepository
                .GetAllQuery()
                .Where(f => f.ClientId == query.ClientId)
                .Select(f => f.PropertyId)
                .ToListAsync(cancellationToken);

            if (!favoritePropertyIds.Any()) return [];

            var properties = await _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => favoritePropertyIds.Contains(p.PropertyId))
                .ToListAsync(cancellationToken);

            var propertyDtos = new List<PropertyDto>();

            foreach (var property in properties)
            {
                var dto = _mapper.Map<PropertyDto>(property);
                var agent = await _authService.GetUserById(property.AgentId);

                if (agent != null)
                {
                    dto.AgentName = $"{agent.Name} {agent.LastName}";
                    dto.AgentEmail = agent.Email;
                    dto.AgentPhone = agent.PhoneNumber;
                    dto.AgentProfileImage = agent.ProfileImage;
                }

                propertyDtos.Add(dto);
            }

            return propertyDtos;
        }
    }
}
