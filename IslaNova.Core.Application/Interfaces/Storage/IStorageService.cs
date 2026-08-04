using Microsoft.AspNetCore.Http;

namespace IslaNova.Core.Application.Interfaces.Storage
{
    public interface IStorageService
    {
        Task<string> UploadAsync(IFormFile file, string bucket, string folder, string fileName);
        Task<List<string>> UploadMultipleAsync(List<IFormFile> files, string bucket, string folder, string fileName);
        Task DeleteAsync(string fileUrl, string bucket);
    }
}
