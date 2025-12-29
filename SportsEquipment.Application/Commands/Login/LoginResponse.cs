using SportsEquipment.Application.DTOs.Users;

namespace SportsEquipment.Application.Commands.Login
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }
}
