using Microsoft.EntityFrameworkCore;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Infrastructure.Data;

namespace SportsEquipment.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação EF Core do IUserRepository.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            if (user == null) 
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
                return null;

            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email.Trim());
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null) 
                throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);

            await Task.CompletedTask;
        }
    }
}
