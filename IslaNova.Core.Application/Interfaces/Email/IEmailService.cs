using IslaNova.Core.Application.Dtos.Email;

namespace IslaNova.Core.Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}
