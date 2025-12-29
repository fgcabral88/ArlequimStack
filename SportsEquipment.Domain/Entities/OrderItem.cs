using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Item de pedido: referência ao produto, quantidade e preço unitário no momento do pedido.
    /// </summary>
    public class OrderItem
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; } = null!;

        protected OrderItem() { }

        public OrderItem(Guid productId, int quantity, Money unitPrice)
        {
            if (productId == Guid.Empty)
                throw new DomainException("ProductId inválido.");

            if (quantity <= 0)
                throw new DomainException("Quantidade do item deve ser maior que zero.");

            if (unitPrice == null)
                throw new DomainException("UnitPrice é obrigatório.");

            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public Money LineTotal() => UnitPrice.Multiply(Quantity);
    }
}
