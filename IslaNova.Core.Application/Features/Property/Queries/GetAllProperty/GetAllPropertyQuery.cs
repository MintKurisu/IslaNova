using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.Property;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Features.Property.Queries.GetAllProperty
{
    /// <summary>
    /// Query used to retrieve all properties available in the system.
    /// </summary>
    public class GetAllPropertyQuery : IRequest<IList<PropertyApiDto>> { }

    public class GetAllPropertyQueryHandler : IRequestHandler<GetAllPropertyQuery, IList<PropertyApiDto>>
    {

        private readonly IAuthServiceForWebApi _authServiceForWebApi;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public GetAllPropertyQueryHandler(IPropertyRepository propertyRepository, IAuthServiceForWebApi authServiceForWebApi, IMapper mapper)
        {
            _authServiceForWebApi = authServiceForWebApi;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<IList<PropertyApiDto>> Handle(GetAllPropertyQuery query, CancellationToken cancellationToken)
        {
            var entityList = await _propertyRepository.GetAllListWithIncludeAsync(["PropertyType", "SaleType", "PropertyImprovements.Improvement"]);
            List<PropertyApiDto> dtoList = [];

            foreach (var entity in entityList)
            {
                var agent = await _authServiceForWebApi.GetUserById(entity.AgentId);
                var dto = _mapper.Map<PropertyApiDto>(entity);

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
