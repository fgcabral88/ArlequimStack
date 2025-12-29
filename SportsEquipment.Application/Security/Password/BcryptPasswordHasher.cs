using SportsEquipment.Application.Security.Interfaces;

namespace SportsEquipment.Application.Security.Password
{
    /// <summary>
    /// Implementação baseada em BCrypt (recomendada para produção).
    /// Adicione o pacote BCrypt.Net-Next no projeto.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly int _workFactor;

        public BcryptPasswordHasher(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
