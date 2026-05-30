using AutoMapper;
using MediatR;
using IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Features.PropertyType.Queries.GetAllPropertyType
{
    /// <summary>
    /// Query used to retrieve all property types available in the system.
    /// </summary>
    public class GetAllPropertyTypeQuery : IRequest<IList<PropertyTypeApiDto>> { }

    public class GetAllPropertyTypeQueryHandler : IRequestHandler<GetAllPropertyTypeQuery, IList<PropertyTypeApiDto>>
    {

        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IMapper _mapper;

        public GetAllPropertyTypeQueryHandler(IPropertyTypeRepository propertyTypeRepository, IMapper mapper)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _mapper = mapper;
        }

        public async Task<IList<PropertyTypeApiDto>> Handle(GetAllPropertyTypeQuery query, CancellationToken cancellationToken)
        {
            var propertyTypeList = await _propertyTypeRepository.GetAllListAsync();

            return _mapper.Map<IList<PropertyTypeApiDto>>(propertyTypeList);
        }
    }
}
