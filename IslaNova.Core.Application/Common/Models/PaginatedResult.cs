namespace IslaNova.Core.Application.Common.Models
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = [];
        public PageMetadata Meta { get; set; } = new();
    }
}
