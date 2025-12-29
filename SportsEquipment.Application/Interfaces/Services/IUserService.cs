using SportsEquipment.Application.Commands.Login;
using SportsEquipment.Application.Commands.Users;
using SportsEquipment.Application.DTOs.Login;
using SportsEquipment.Application.DTOs.Users;

namespace SportsEquipment.Application.Interfaces.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Registra um usuário (Admin ou Seller).
        /// </summary>
        Task<UserDto> RegisterAsync(CreateUserCommand command);

        /// <summary>
        /// Autentica um usuário por e-mail/senha e retorna token + info.
        /// A geração do token é delegada a infraestrutura (ex.: IJwtService) via implementação concreta.
        /// </summary>
        Task<AuthenticateResult> AuthenticateAsync(LoginRequest request);

        Task<UserDto> GetByIdAsync(Guid id);
    }
}
