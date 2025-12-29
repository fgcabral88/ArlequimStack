using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
