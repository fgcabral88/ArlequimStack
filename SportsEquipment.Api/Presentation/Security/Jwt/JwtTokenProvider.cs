using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SportsEquipment.Api.Presentation.Configuration.Jwt;
using SportsEquipment.Application.Security.Interfaces;
using SportsEquipment.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SportsEquipment.Api.Presentation.Security.Jwt
{
    /// <summary>
    /// Gera JWT para o usuário usando JwtSettings (injetado via IConfiguration).
    /// Implementa o ITokenProvider usado pela camada Application.
    /// </summary>
    public class JwtTokenProvider : ITokenProvider
    {
        private readonly JwtSettings _settings;
        private readonly byte[] _keyBytes;

        public JwtTokenProvider(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(_settings.Secret))
                throw new ArgumentException("JWT secret is required in configuration.");

            _keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        }

        /// <summary>
        /// Gera o token JWT com claims básicos (sub, email, name, role).
        /// </summary>
        public string GenerateToken(User user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_settings.ExpiryMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name),
                new Claim(ClaimTypes.Role, user.Type.ToString())
            };

            var signingKey = new SymmetricSecurityKey(_keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TimeSpan TokenLifetime => TimeSpan.FromMinutes(_settings.ExpiryMinutes);
    }
}
