using SportsEquipment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using SportsEquipment.Application.Interfaces.Repositories;

namespace SportsEquipment.Infrastructure.Repositories
{
    /// <summary>
    /// UnitOfWork que usa o ApplicationDbContext para controlar transações.
    /// </summary>
    public class EfUnitOfWork : IUnitOfWork, IDisposable 
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public EfUnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null) 
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) 
                return;

            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) 
                return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
