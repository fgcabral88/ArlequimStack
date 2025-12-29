using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SportsEquipment.Application.DTOs.Users;
using SportsEquipment.Application.Commands.Login;
using SportsEquipment.Application.Commands.Users;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de usuários e autenticação")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra um novo usuário (Admin ou Seller).
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Registra um novo usuário", Description = "Cria uma nova conta de usuário no sistema. Pode ser Administrador ou Vendedor.")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Usuário registrado com sucesso", typeof(UserDto))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Dados inválidos ou e-mail já em uso")]
        public async Task<IActionResult> RegisterAsync([FromBody] CreateUserCommand command)
        {
            var user = await _userService.RegisterAsync(command);

           // return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, user);
            return CreatedAtAction("GetById", new { id = user.Id }, user);
        }

        /// <summary>
        /// Login via e-mail. Retorna token e dados do usuário.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Autentica um usuário", Description = "Realiza login com e-mail e senha, retornando token JWT e informações do usuário.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Login realizado com sucesso", typeof(LoginResponse))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Credenciais inválidas")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var result = await _userService.AuthenticateAsync(request);

            return Ok(result);
        }

        /// <summary>
        /// Retorna usuário por id. Administradores e o próprio dono do token podem acessar.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Obtém usuário por Id", Description = "Retorna informações de um usuário específico. Apenas Administradores ou o próprio usuário podem acessar.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Usuário encontrado", typeof(UserDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Usuário não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão para acessar este recurso")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var userDto = await _userService.GetByIdAsync(id);

            if (User.IsInRole("Administrator"))
                return Ok(userDto);

            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (Guid.TryParse(sub, out var subjectId) && subjectId == userDto.Id)
                return Ok(userDto);

            return Forbid();
        }
    }
}
