using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Core.Application.Features.Property.Queries.FilterProperties
{
    public class FilterPropertiesQuery : IRequest<IList<PropertyDto>>
    {
        [SwaggerParameter(Description = "Filter by property type ID")]
        public int? PropertyTypeId { get; set; }
        [SwaggerParameter(Description = "Minimum price")]
        public decimal? MinPrice { get; set; }
        [SwaggerParameter(Description = "Maximum price")]
        public decimal? MaxPrice { get; set; }
        [SwaggerParameter(Description = "Number of bedrooms")]
        public int? Bedrooms { get; set; }
        [SwaggerParameter(Description = "Number of bathrooms")]
        public int? Bathrooms { get; set; }
    }

    public class FilterPropertiesQueryHandler : IRequestHandler<FilterPropertiesQuery, IList<PropertyDto>>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAuthServiceForWebApi _authService;
        private readonly IMapper _mapper;

        public FilterPropertiesQueryHandler(
            IPropertyRepository propertyRepository,
            IAuthServiceForWebApi authService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<IList<PropertyDto>> Handle(FilterPropertiesQuery query, CancellationToken cancellationToken)
        {
            var dbQuery = _propertyRepository
                .GetAllQueryWithInclude(["PropertyType", "SaleType", "Images", "PropertyImprovements.Improvement"])
                .Where(p => p.Status == PropertyStatus.Available);

            if (query.PropertyTypeId.HasValue)
                dbQuery = dbQuery.Where(p => p.PropertyTypeId == query.PropertyTypeId.Value);

            if (query.MinPrice.HasValue)
                dbQuery = dbQuery.Where(p => p.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                dbQuery = dbQuery.Where(p => p.Price <= query.MaxPrice.Value);

            if (query.Bedrooms.HasValue)
                dbQuery = dbQuery.Where(p => p.Bedrooms == query.Bedrooms.Value);

            if (query.Bathrooms.HasValue)
                dbQuery = dbQuery.Where(p => p.Bathrooms == query.Bathrooms.Value);

            var properties = await dbQuery
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