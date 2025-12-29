using SportsEquipment.Domain.Enums;

namespace SportsEquipment.Application.Commands.Users
{
    public class CreateUserCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType Type { get; set; } = UserType.Seller;
    }
}
