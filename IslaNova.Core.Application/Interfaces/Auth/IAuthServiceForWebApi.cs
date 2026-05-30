using IslaNova.Core.Application.Dtos.Auth;

namespace IslaNova.Core.Application.Interfaces.Auth
{
    public interface IAuthServiceForWebApi : IBaseAuthService
    {
        Task<LoginResponseForApiDto> LoginAsync(LoginDto loginDto);
        Task<SignUpResponseDto> SignUpAsync(SignUpDto dto);
        Task<ForgotPasswordResponseDto> ResetPasswordApiAsync(ResetPasswordApiRequestDto request);
    }
}
