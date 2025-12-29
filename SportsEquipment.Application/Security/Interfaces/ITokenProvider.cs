using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Application.Security.Interfaces
{
    /// <summary>
    /// Contrato mínimo para gerar tokens — a implementação concreta fica na camada API/Infra.
    /// </summary>
    public interface ITokenProvider
    {
        string GenerateToken(User user);
        TimeSpan TokenLifetime { get; }
    }
}
