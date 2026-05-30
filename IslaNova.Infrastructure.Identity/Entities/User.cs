using Microsoft.AspNetCore.Identity;

namespace IslaNova.Infrastructure.Identity.Entities;
public class User : IdentityUser
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string IdentificationNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProfileImage { get; set; }
}
