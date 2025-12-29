using Microsoft.EntityFrameworkCore;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Infrastructure.Data;

namespace SportsEquipment.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Order order)
        {
            if (order == null) 
                throw new ArgumentNullException(nameof(order));

            await _context.Orders.AddAsync(order);
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(nameof(Order.Items))
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateAsync(Order order)
        {
            if (order == null) 
                throw new ArgumentNullException(nameof(order));

            _context.Orders.Update(order);

            await Task.CompletedTask;
        }
    }
}
