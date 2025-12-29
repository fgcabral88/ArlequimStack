namespace SportsEquipment.Domain.Common
{
    /// <summary>
    /// Entidade base com identificador e timestamp de criação.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Identificador único da entidade.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Data de criação (UTC).
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
