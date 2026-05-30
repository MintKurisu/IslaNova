using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Domain.Common.Enums;

namespace IslaNova.Core.Application.Interfaces.Auth
{
    public interface IBaseAuthService
    {
        Task<AddUserResponseDto> AddUserAsync(AddUserDto dto);
        Task<bool> ToggleUserStatus(string id, bool? status = null);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<DeleteResponseDto> DeleteAsync(string id);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false);
        Task<List<UserDto>> GetAllUser(bool? isActive = true);
        Task<List<UserDto>> GetAllUserByRole(Roles role);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserById(string Id);
        Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<UpdateUserResponseDto> UpdateUserAsync(UpdateUserDto dto, bool? isCreated);
    }
}