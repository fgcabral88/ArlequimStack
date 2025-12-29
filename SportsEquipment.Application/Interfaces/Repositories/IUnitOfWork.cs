namespace SportsEquipment.Application.Interfaces.Repositories
{
    /// <summary>
    /// Unidade de trabalho para coordenar transações entre repositórios.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
    }
}
