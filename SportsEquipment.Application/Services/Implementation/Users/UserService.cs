using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Application.DTOs.Login;
using SportsEquipment.Application.DTOs.Users;
using SportsEquipment.Application.Commands.Login;
using SportsEquipment.Application.Commands.Users;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Security.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;

namespace SportsEquipment.Application.Services.Implementation.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenProvider? _tokenProvider;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, ILogger<UserService> logger, ITokenProvider? tokenProvider = null)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<UserDto> RegisterAsync(CreateUserCommand command)
        {
            _logger.LogInformation("Iniciando registro de usuário. Email: {Email}, Nome: {Name}, Tipo: {Type}", command.Email, command.Name, command.Type);

            if (command is null)
            {
                _logger.LogError("Command de registro de usuário é nulo");

                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(command.Email))
            {
                _logger.LogError("E-mail é obrigatório para registro");

                throw new DomainException("E-mail é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            {
                _logger.LogError("Senha deve ter no mínimo 6 caracteres. Tamanho atual: {PasswordLength}", command.Password?.Length ?? 0);

                throw new DomainException("Senha deve ter no mínimo 6 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                _logger.LogError("Nome é obrigatório para registro");

                throw new DomainException("Nome é obrigatório.");
            }

            var emailTrimmed = command.Email.Trim();

            _logger.LogDebug("Verificando se email {Email} já existe", emailTrimmed);

            var exists = await _userRepository.GetByEmailAsync(emailTrimmed);

            if (exists != null)
            {
                _logger.LogWarning("Tentativa de registro com email já existente: {Email}", emailTrimmed);

                throw new DomainException("Já existe usuário com este e-mail.");
            }

            var hash = _passwordHasher.Hash(command.Password);
            var user = new User(command.Name.Trim(), emailTrimmed, hash, command.Type);

            _logger.LogDebug("Usuário criado em memória. Hash gerado: {HashLength} caracteres", hash.Length);

            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Usuário registrado com sucesso. ID: {UserId}, Email: {Email}, Tipo: {Type}", user.Id, user.Email, user.Type);

                return new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Type = user.Type
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante registro de usuário. Erro: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<AuthenticateResult> AuthenticateAsync(LoginRequest request)
        {
            _logger.LogInformation("Iniciando autenticação para email: {Email}", request.Email);

            if (request is null)
            {
                _logger.LogError("Request de autenticação é nulo");

                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogError("Email ou senha não fornecidos para autenticação");

                throw new DomainException("E-mail e senha são obrigatórios.");
            }

            var emailTrimmed = request.Email.Trim();

            _logger.LogDebug("Buscando usuário por email: {Email}", emailTrimmed);

            var user = await _userRepository.GetByEmailAsync(emailTrimmed);

            if (user is null)
            {
                _logger.LogWarning("Tentativa de login com email não encontrado: {Email}", emailTrimmed);

                throw new DomainException("Credenciais inválidas.");
            }

            _logger.LogDebug("Usuário encontrado. ID: {UserId}, Nome: {UserName}", user.Id, user.Name);

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Senha incorreta para usuário: {Email}", emailTrimmed);

                throw new DomainException("Credenciais inválidas.");
            }

            _logger.LogDebug("Senha verificada com sucesso para usuário: {Email}", emailTrimmed);

            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Type = user.Type
            };

            if (_tokenProvider == null)
            {
                _logger.LogWarning("TokenProvider não configurado. Retornando autenticação sem token (modo teste)");

                return new AuthenticateResult
                {
                    Token = string.Empty,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow
                };
            }

            var token = _tokenProvider.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.Add(_tokenProvider.TokenLifetime);

            _logger.LogInformation("Autenticação bem-sucedida. Usuário: {Email}, ID: {UserId}, Token gerado: {TokenLength} caracteres, Expira em: {ExpiresAt}", user.Email, user.Id, token.Length, expiresAt);

            return new AuthenticateResult
            {
                Token = token,
                User = userDto,
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Buscando usuário por ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
            {
                _logger.LogWarning("Usuário não encontrado. ID: {UserId}", id);

                throw new DomainException("Usuário não encontrado.");
            }

            _logger.LogDebug("Usuário encontrado. ID: {UserId}, Nome: {UserName}, Email: {Email}", user.Id, user.Name, user.Email);

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Type = user.Type
            };
        }
    }
}