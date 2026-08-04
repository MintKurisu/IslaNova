using IslaNova.Core.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Http;
using Supabase;

namespace IslaNova.Infrastructure.Shared.Services
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly Client _supabaseClient;

        public SupabaseStorageService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<string> UploadAsync(IFormFile file, string bucket, string folder, string fileName)
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);

            var filePath = $"{folder}/{fileName}{Path.GetExtension(file.FileName)}";

            await _supabaseClient.Storage
                .From(bucket)
                .Upload(buffer, filePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true,
                    ContentType = file.ContentType
                });

            return _supabaseClient.Storage
                .From(bucket)
                .GetPublicUrl(filePath);
        }

        public async Task<List<string>> UploadMultipleAsync(List<IFormFile> files, string bucket, string folder, string fileName)
        {
            var imageUrls = new List<string>();

            foreach (var file in files)
            {
                var uniqueFileName = $"{fileName}_{Guid.NewGuid()}";
                var url = await UploadAsync(file, bucket, folder, uniqueFileName);
                imageUrls.Add(url);
            }

            return imageUrls;
        }

        public async Task DeleteAsync(string fileUrl, string bucket)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath
                .Replace($"/storage/v1/object/public/{bucket}/", "");

            await _supabaseClient.Storage
                .From(bucket)
                .Remove(new List<string> { path });
        }
    }
}
