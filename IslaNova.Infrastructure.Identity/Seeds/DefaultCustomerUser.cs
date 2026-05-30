using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IslaNova.Core.Domain.Common.Enums;
using IslaNova.Infrastructure.Identity.Entities;

namespace IslaNova.Infrastructure.Identity.Seeds
{
    public static class DefaultCustomerUser
    {
        public static async Task SeedAsync(UserManager<User> userManager)
        {
            User user = new()
            {
                Name = "Steven",
                LastName = "Stone",
                Email = "Steven@example.com",
                EmailConfirmed = true,
                UserName = "basic_customer",
                PhoneNumberConfirmed = true,
                IdentificationNumber = "01012748311",
                CreatedAt = DateTime.UtcNow

            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                if (!await userManager.Users
                    .AnyAsync(u => u.Email == user.Email || u.IdentificationNumber == user.IdentificationNumber))
                {
                    await userManager.CreateAsync(user, "Pa$$word123");
                    await userManager.AddToRoleAsync(user, Roles.Customer.ToString());
                }
            }

        }
    }
}
