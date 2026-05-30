using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;

namespace IslaNova.Infrastructure.Identity.Seeds
{
    public class DefaultAgentUser
    {
        public static async Task SeedAsync(UserManager<User> userManager)
        {
            User user = new()
            {
                Name = "Aaron",
                LastName = "Judge",
                Email = "Aaron@example.com",
                EmailConfirmed = true,
                UserName = "basic_agent",
                PhoneNumberConfirmed = true,
                IdentificationNumber = "01012748782",
                CreatedAt = DateTime.UtcNow

            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                if (!await userManager.Users
                    .AnyAsync(u => u.Email == user.Email || u.IdentificationNumber == user.IdentificationNumber))
                {
                    await userManager.CreateAsync(user, "Pa$$word123");
                    await userManager.AddToRoleAsync(user, Roles.Agent.ToString());
                }
            }

        }
    }
}
