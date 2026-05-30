using IslaNova.Core.Application.Dtos.PropertyManagement.Base;

namespace IslaNova.Core.Application.Dtos.PropertyManagement.PropertyType
{
    // Used to display property type information in listings and details
    public class PropertyTypeDto : BaseTypeDto
    {
        public int PropertyTypeId { get; set; }
        public int PropertyCount { get; set; }
    }
}
