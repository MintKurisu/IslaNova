namespace IslaNova.Core.Application.Dtos.User
{
    public class AgentUserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public int PropertyCount { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
