using Microsoft.EntityFrameworkCore;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Infrastructure.Data;

namespace SportsEquipment.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product)
        {
            if (product == null) 
                throw new ArgumentNullException(nameof(product));

            await _context.Products.AddAsync(product);
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (existing != null)
            {
                _context.Products.Remove(existing);
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Product product)
        {
            if (product == null) 
                throw new ArgumentNullException(nameof(product));

            _context.Products.Update(product);

            await Task.CompletedTask;
        }
    }
}
