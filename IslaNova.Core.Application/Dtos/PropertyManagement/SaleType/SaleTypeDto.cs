using IslaNova.Core.Application.Dtos.PropertyManagement.Base;

namespace IslaNova.Core.Application.Dtos.PropertyManagement.SaleType
{
    // Used to display sale type information (rent, sale, etc.) in listings and details
    public class SaleTypeDto : BaseTypeDto
    {
        public int SaleTypeId { get; set; }
        public int PropertyCount { get; set; }
    }
}
