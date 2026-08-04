using IslaNova.Core.Application.Dtos.Auth;
using IslaNova.Core.Application.Dtos.Email;
using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Application.Interfaces.Auth;
using IslaNova.Core.Application.Interfaces.Email;
using IslaNova.Core.Application.Interfaces.Storage;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace IslaNova.Infrastructure.Identity.Service
{
    public abstract class BaseAuthService : IBaseAuthService
    {

        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IStorageService _storageService;

        protected BaseAuthService(
            UserManager<User> userManager,
            IEmailService emailService,
            IStorageService storageService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _storageService = storageService;
        }

        public virtual async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            ForgotPasswordResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this username {request.UserName}");
                return response;
            }

            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            if (isApi != null && !isApi.Value)
            {
                var resetUri = await GetResetPasswordUri(user, request.Origin ?? "");
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account visiting this URL {resetUri}",
                    Subject = "Reset password"
                });
            }
            else
            {
                string? resetToken = await GetResetPasswordToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Please reset your password account use this token {resetToken}",
                    Subject = "Reset password"
                });
            }

            return response;
        }

        public virtual async Task<ForgotPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            ForgotPasswordResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this user");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }

        public virtual async Task<string> ConfirmAccountAsync(string userId, string token)
        {

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return "There's no account registered with this user";
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return $"Account confirmed for {user.Email}. You can now use the app";
            }
            else
            {
                return $"An error occurred while confirming this email {user.Email}";
            }

        }

        public virtual async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
            {
                users = users.Where(w => w.EmailConfirmed);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var rolesList = await _userManager.GetRolesAsync(item);

                listUsersDtos.Add(new UserDto()
                {
                    Id = item.Id,
                    Email = item.Email ?? "",
                    LastName = item.LastName,
                    Name = item.Name,
                    UserName = item.UserName ?? "",
                    IdentificationNumber = item.IdentificationNumber,
                    PhoneNumber = item.PhoneNumber ?? "",
                    Role = rolesList.FirstOrDefault() ?? "",
                    IsActive = item.EmailConfirmed,
                    ProfileImage = item.ProfileImage
                });
            }

            return listUsersDtos;
        }

        public virtual async Task<List<UserDto>> GetAllUserByRole(Roles role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role.ToString());

            List<UserDto> listUsersDtos = [];

            foreach (var user in users)
            {

                listUsersDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    LastName = user.LastName,
                    Name = user.Name,
                    UserName = user.UserName ?? "",
                    IdentificationNumber = user.IdentificationNumber,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = role.ToString(),
                    IsActive = user.EmailConfirmed,
                    ProfileImage = user.ProfileImage
                });
            }

            return listUsersDtos;
        }

        public virtual async Task<UserDto?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber ?? "",
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed,
                ProfileImage = user.ProfileImage
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber ?? "",
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed,
                ProfileImage = user.ProfileImage
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber ?? "",
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed,
                ProfileImage = user.ProfileImage
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber)
        {
            var users = _userManager.Users;
            var user = await users.FirstOrDefaultAsync(u => u.IdentificationNumber == identificationNumber);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber ?? "",
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed,
                ProfileImage = user.ProfileImage
            };

            return userDto;
        }

        public virtual async Task<AddUserResponseDto> AddUserAsync(AddUserDto dto)
        {
            AddUserResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                IdentificationNumber = "",
                ProfileImage = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(dto.UserName);

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"Username {dto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"Email {dto.Email} is already taken.");
                return response;
            }

            var userWithSameIdentificationNumber = await _userManager.Users.FirstOrDefaultAsync(w => w.IdentificationNumber == dto.IdentificationNumber);

            if (userWithSameIdentificationNumber != null)
            {
                response.HasError = true;
                response.Errors.Add($"this identification number: {dto.IdentificationNumber} is already taken.");
                return response;
            }

            User user = new()
            {
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.UserName,
                IdentificationNumber = dto.IdentificationNumber,
                ProfileImage = dto.ProfileImage,
                EmailConfirmed = false,
            };

            // Admin users are created active
            if (dto.Role == Roles.Admin.ToString())
            {
                user.EmailConfirmed = true;
            }


            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, dto.Role);

                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IdentificationNumber = user.IdentificationNumber;
                response.Roles = rolesList.ToList();
                response.IsActive = user.EmailConfirmed;

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public virtual async Task<UpdateUserResponseDto> UpdateUserAsync(UpdateUserDto dto, bool? isCreated)
        {
            bool isNotcreated = !isCreated ?? false;
            UpdateUserResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                IdentificationNumber = "",
                Role = "",
                HasError = false,
                Errors = []
            };

            string? newImageUrl = null;
            var existingProfileImageUrl = string.Empty;

            var user = await _userManager.FindByIdAsync(dto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this user");
                return response;
            }

            var userWithSameUserName = await _userManager.Users
                .FirstOrDefaultAsync(w => w.NormalizedUserName == dto.UserName.ToUpper() && w.Id != dto.Id);

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {dto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.Users
                .FirstOrDefaultAsync(w => w.NormalizedEmail == dto.Email.ToUpper() && w.Id != dto.Id);

            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {dto.Email} is already taken.");
                return response;
            }

            var userWithSameIdentificationNumber = await _userManager.Users
                .FirstOrDefaultAsync(w => w.IdentificationNumber == dto.IdentificationNumber && w.Id != dto.Id);

            if (userWithSameIdentificationNumber != null)
            {
                response.HasError = true;
                response.Errors.Add($"this identification number: {dto.IdentificationNumber} is already taken.");
                return response;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            if (dto.ProfileImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString();
                newImageUrl = await _storageService.UploadAsync(dto.ProfileImageFile, "profile-images", "agents", fileName);
                existingProfileImageUrl = user.ProfileImage ?? string.Empty;
            }


            user.Name = dto.Name;
            user.LastName = dto.LastName;
            user.UserName = user.UserName;
            user.IdentificationNumber = dto.IdentificationNumber;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.Email = dto.Email;
            user.ProfileImage = newImageUrl ?? user.ProfileImage;

            if (!string.IsNullOrWhiteSpace(dto.Password) && isNotcreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var changePasswordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                if (!changePasswordResult.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.Add($"Error updating password");
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            // Rollback: Delete the newly uploaded image if user update fails
            if (!result.Succeeded && newImageUrl != null)
                await _storageService.DeleteAsync(newImageUrl, "profile-iamges");


            // If new image is uploaded delete old image from cloudinary
            if (!string.IsNullOrEmpty(existingProfileImageUrl) && newImageUrl != null)
            {
                await _storageService.DeleteAsync(existingProfileImageUrl, "profile-iamges");
            }



            if (result.Succeeded)
            {
                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsActive = user.EmailConfirmed;
                response.Role = roleList.FirstOrDefault() ?? "";
                response.IdentificationNumber = user.IdentificationNumber;
                response.ProfileImage = user.ProfileImage;

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }

        public virtual async Task<DeleteResponseDto> DeleteAsync(string id)
        {
            DeleteResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no account registered with this user");
                return response;
            }

            await _userManager.DeleteAsync(user);

            return response;
        }

        public virtual async Task<bool> ToggleUserStatus(string id, bool? status = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            bool shouldBeActive = status ?? !user.EmailConfirmed;

            if (shouldBeActive)
            {

                user.EmailConfirmed = true;

                await _userManager.SetLockoutEndDateAsync(user, null);

                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else
            {
                user.EmailConfirmed = false;

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                user.LockoutEnabled = true;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }



        #region Protected methods
        protected async Task<string> GetVerificationEmailUri(User user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Auth/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", encodedToken);

            return verificationUri;
        }
        protected async Task<string> GetResetPasswordUri(User user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Auth/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetVerificationEmailToken(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        protected async Task<string?> GetResetPasswordToken(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        #endregion
    }
}
