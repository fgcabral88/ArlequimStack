using SportsEquipment.Domain.Common;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Registro de entrada de estoque (para auditoria).
    /// Cada entrada contém a quantidade recebida e o número da nota fiscal.
    /// </summary>
    public class StockEntry : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public string FiscalNoteNumber { get; private set; } = string.Empty;
        public DateTime RegisteredAt { get; private set; }

        protected StockEntry() { }

        public StockEntry(Guid productId, int quantity, string fiscalNoteNumber)
        {
            if (productId == Guid.Empty)
                throw new DomainException("ProductId inválido.");

            if (quantity <= 0)
                throw new DomainException("Quantidade deve ser maior que zero.");

            if (string.IsNullOrWhiteSpace(fiscalNoteNumber))
                throw new DomainException("Número da nota fiscal é obrigatório.");

            ProductId = productId;
            Quantity = quantity;
            FiscalNoteNumber = fiscalNoteNumber.Trim();
            RegisteredAt = CreatedAt;
        }
    }
}
