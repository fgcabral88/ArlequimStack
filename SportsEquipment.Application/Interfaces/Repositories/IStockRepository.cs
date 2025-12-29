using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Application.Interfaces.Repositories
{
    public interface IStockRepository
    {
        Task<ProductStock?> GetByProductIdAsync(Guid productId);
        Task AddAsync(ProductStock stock);
        Task UpdateAsync(ProductStock stock);
    }
}
