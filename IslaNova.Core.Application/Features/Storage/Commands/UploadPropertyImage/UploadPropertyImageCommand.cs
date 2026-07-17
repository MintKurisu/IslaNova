using IslaNova.Core.Application.Interfaces.Storage;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IslaNova.Core.Application.Features.Storage.Commands.UploadPropertyImage
{
    public class UploadPropertyImageCommand : IRequest<string?>
    {
        public IFormFile File { get; set; } = null!;
    }

    public class UploadPropertyImageCommandHandler : IRequestHandler<UploadPropertyImageCommand, string?>
    {
        private readonly IStorageService _storageService;

        public UploadPropertyImageCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<string?> Handle(UploadPropertyImageCommand command, CancellationToken cancellationToken)
        {
            var fileName = Guid.NewGuid().ToString();
            return await _storageService.UploadAsync(command.File, "property-images", "properties", fileName);
        }
    }
}
