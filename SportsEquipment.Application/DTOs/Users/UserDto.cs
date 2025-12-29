using SportsEquipment.Domain.Enums;

namespace SportsEquipment.Application.DTOs.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserType Type { get; set; }
    }
}
