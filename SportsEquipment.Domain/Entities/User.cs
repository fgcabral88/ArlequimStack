using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Enums;
using System.Text.RegularExpressions;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Representa um usuário do sistema (administrador ou vendedor).
    /// Observação: a senha deve ser armazenada como hash (não implementado aqui).
    /// </summary>
    public class User : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        /// <summary>
        /// Hash da senha (ex.: bcrypt). Nunca armazene a senha em texto puro.
        /// </summary>
        public string PasswordHash { get; private set; } = string.Empty;
        public UserType Type { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected User() { }

        public User(string name, string email, string passwordHash, UserType type)
        {
            SetName(name);
            SetEmail(email);
            SetPasswordHash(passwordHash);
            Type = type;
            UpdatedAt = CreatedAt;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Nome do usuário é obrigatório.");

            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("E-mail é obrigatório.");

            var trimmed = email.Trim();
            if (!IsValidEmail(trimmed))
                throw new DomainException("E-mail em formato inválido.");

            Email = trimmed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("Password hash é obrigatório.");

            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeType(UserType newType)
        {
            Type = newType;
            UpdatedAt = DateTime.UtcNow;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch
            {
                return false;
            }
        }
    }
}
