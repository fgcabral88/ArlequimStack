using SportsEquipment.Domain.Common;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Agregado que representa o estoque corrente de um produto e permite operações de adição/remoção.
    /// Mantém um registro de entradas de estoque para auditoria (StockEntry).
    /// </summary>
    public class ProductStock : BaseEntity
    {
        public Guid ProductId { get; private set; }
        private readonly List<StockEntry> _entries = new List<StockEntry>();
        public IReadOnlyCollection<StockEntry> Entries => _entries.AsReadOnly();
        public int CurrentQuantity { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected ProductStock() { }

        public ProductStock(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new DomainException("ProductId inválido.");

            ProductId = productId;
            CurrentQuantity = 0;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Adiciona estoque a este produto e registra a nota fiscal.
        /// </summary>
        /// <param name="quantity">Quantidade a adicionar (maior que 0).</param>
        /// <param name="fiscalNoteNumber">Número da nota fiscal (obrigatório).</param>
        /// <returns>O StockEntry criado (para persistência/auditoria).</returns>
        public StockEntry AddStock(int quantity, string fiscalNoteNumber)
        {
            if (quantity <= 0)
                throw new DomainException("Quantidade a adicionar deve ser maior que zero.");

            if (string.IsNullOrWhiteSpace(fiscalNoteNumber))
                throw new DomainException("Número da nota fiscal é obrigatório.");

            var entry = new StockEntry(ProductId, quantity, fiscalNoteNumber);

            _entries.Add(entry);
            CurrentQuantity += quantity;
            UpdatedAt = DateTime.UtcNow;

            return entry;
        }

        /// <summary>
        /// Remove (dar baixa) quantidade do estoque. Lança DomainException se não houver quantidade suficiente.
        /// </summary>
        /// <param name="quantity">Quantidade a remover.</param>
        public void RemoveStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantidade a remover deve ser maior que zero.");

            if (quantity > CurrentQuantity)
                throw new DomainException("Estoque insuficiente para a operação.");

            CurrentQuantity -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Retorna a quantidade disponível no estoque.
        /// </summary>
        /// <returns>Quantidade disponível (int)</returns>
        public int GetAvailableQuantity() => CurrentQuantity;
    }
}
