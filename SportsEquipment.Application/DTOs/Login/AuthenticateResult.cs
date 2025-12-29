using SportsEquipment.Application.DTOs.Users;

namespace SportsEquipment.Application.DTOs.Login
{
    public class AuthenticateResult
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
        public DateTime ExpiresAt { get; set; }
    }
}
