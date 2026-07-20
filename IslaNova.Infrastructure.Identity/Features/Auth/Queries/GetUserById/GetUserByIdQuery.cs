using IslaNova.Core.Application.Dtos.User;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace IslaNova.Infrastructure.Identity.Features.Auth.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        [SwaggerParameter(Description = "Identifier of the user to login. Either Email or Username")]
        public required string UserId { get; set; }
    }


    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly UserManager<User> _userManager;

        public GetUserByIdQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(query.UserId);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var roleString = rolesList.FirstOrDefault();

            if (!Enum.TryParse<Roles>(roleString, out var role))
            {
                throw new Exception($"Invalid role '{roleString}'");
            }

            var userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email ?? "",
                IdentificationNumber = user.IdentificationNumber,
                PhoneNumber = user.PhoneNumber ?? "",
                Role = role.ToString(),
                UserName = user.UserName ?? "",
                ProfileImage = user.ProfileImage ?? ""
            };


            return userDto;
        }
    }

}