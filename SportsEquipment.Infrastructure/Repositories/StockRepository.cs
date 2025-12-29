using Microsoft.EntityFrameworkCore;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Infrastructure.Data;

namespace SportsEquipment.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProductStock stock)
        {
            if (stock == null) 
                throw new ArgumentNullException(nameof(stock));

            await _context.ProductStocks.AddAsync(stock);
        }

        public async Task<ProductStock?> GetByProductIdAsync(Guid productId)
        {
            // Use Include of entries via EF (backing field)
            return await _context.ProductStocks
                .Include(nameof(ProductStock.Entries))
                .FirstOrDefaultAsync(ps => ps.ProductId == productId);
        }

        public async Task UpdateAsync(ProductStock stock)
        {
            if (stock == null) 
                throw new ArgumentNullException(nameof(stock));

            _context.ProductStocks.Update(stock);

            await Task.CompletedTask;
        }
    }
}
