using IslaNova.Core.Application.Interfaces.Storage;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace IslaNova.Core.Application.Features.Storage.Commands.UploadProfileImage
{
    public class UploadProfileImageCommand : IRequest<string?>
    {
        public IFormFile File { get; set; } = null!;
    }

    public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, string?>
    {
        private readonly IStorageService _storageService;

        public UploadProfileImageCommandHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<string?> Handle(UploadProfileImageCommand command, CancellationToken cancellationToken)
        {
            var fileName = Guid.NewGuid().ToString();
            return await _storageService.UploadAsync(command.File, "profile-images", "agents", fileName);
        }
    }
}
