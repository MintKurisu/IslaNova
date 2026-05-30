using AutoMapper;
using IslaNova.Core.Application.Dtos.PropertyManagement.SaleType;
using IslaNova.Core.Application.Interfaces.PropertyManagement;
using IslaNova.Core.Application.Services.Base;
using IslaNova.Core.Domain.Entities.PropertyManagement;
using IslaNova.Core.Domain.Interfaces.PropertyManagement;

namespace IslaNova.Core.Application.Services.PropertyManagement
{
    public class SaleTypeService : GenericService<SaleType, SaleTypeDto>, ISaleTypeService
    {
        private readonly ISaleTypeRepository _saleTypeRepository;
        private readonly IMapper _mapper;

        public SaleTypeService(ISaleTypeRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
            _saleTypeRepository = repository;
            _mapper = mapper;
        }

        public override async Task<List<SaleTypeDto>> GetAllAsync()
        {
            try
            {
                var saleTypes = await _saleTypeRepository.GetAllListWithIncludeAsync(
                    new List<string> { "Properties" }
                );

                var saleTypeDtos = _mapper.Map<List<SaleTypeDto>>(saleTypes);
                return saleTypeDtos;
            }
            catch (Exception)
            {
                return new List<SaleTypeDto>();
            }
        }

        public override async Task<SaleTypeDto?> GetByIdAsync(int id)
        {
            try
            {
                var saleType = await _saleTypeRepository.GetByIdWithIncludeAsync(
                    id,
                    new List<string> { "Properties" } 
                );

                if (saleType == null)
                {
                    return null;
                }

                var saleTypeDto = _mapper.Map<SaleTypeDto>(saleType);

                if (saleTypeDto != null)
                {
                    saleTypeDto.PropertyCount = saleType.Properties?.Count ?? 0;
                }

                return saleTypeDto;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
    }
}

