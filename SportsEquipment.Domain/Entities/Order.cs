using SportsEquipment.Domain.Enums;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Pedido de venda gerado por um vendedor.
    /// Possui overloads para AddItem(OrderItem) e AddItem(productId, quantity, unitPrice) para facilitar uso na aplicação e em testes.
    /// </summary>
    public class Order : BaseEntity
    {
        public string ClientDocument { get; private set; } = string.Empty;
        public string SellerName { get; private set; } = string.Empty;
        private readonly List<OrderItem> _items = new List<OrderItem>();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        public OrderStatus Status { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected Order()
        {
            Status = OrderStatus.Draft;
            UpdatedAt = CreatedAt;
        }

        public Order(string clientDocument, string sellerName)
        {
            SetClientDocument(clientDocument);
            SetSellerName(sellerName);
            Status = OrderStatus.Draft;
            UpdatedAt = CreatedAt;
        }

        public void SetClientDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                throw new DomainException("Documento do cliente é obrigatório.");

            ClientDocument = document.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSellerName(string sellerName)
        {
            if (string.IsNullOrWhiteSpace(sellerName))
                throw new DomainException("Nome do vendedor é obrigatório.");

            SellerName = sellerName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adiciona um OrderItem existente (útil quando já foi criado externamente).
        /// Mantém a regra que não permite items duplicados para o mesmo produto.
        /// </summary>
        /// <param name="item">Item já construído</param>
        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new DomainException("Item inválido.");

            if (_items.Any(i => i.ProductId == item.ProductId))
                throw new DomainException("Item já adicionado. Atualize a quantidade em vez de adicionar duplicado.");

            _items.Add(item);
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Overload de conveniência para criar e adicionar um item a partir de dados primitivos.
        /// Lança DomainException se quantity <= 0 (OrderItem valida isso).
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <param name="unitPrice"></param>
        public void AddItem(Guid productId, int quantity, Money unitPrice)
        {
            var item = new OrderItem(productId, quantity, unitPrice);

            AddItem(item);
        }

        public void RemoveItem(Guid productId)
        {
            var existing = _items.FirstOrDefault(i => i.ProductId == productId);

            if (existing == null)
                throw new DomainException("Item não encontrado no pedido.");

            _items.Remove(existing);
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal TotalAmount()
        {
            return _items.Sum(i => i.UnitPrice.Amount * i.Quantity);
        }

        public void ValidateAvailability(Func<Guid, int> availableProvider)
        {
            if (availableProvider == null)
                throw new ArgumentNullException(nameof(availableProvider));

            foreach (var item in _items)
            {
                var available = availableProvider(item.ProductId);

                if (item.Quantity > available)
                    throw new DomainException($"Estoque insuficiente para o produto {item.ProductId}. Disponível: {available}, requerido: {item.Quantity}.");
            }
        }

        public void Confirm()
        {
            if (!_items.Any())
                throw new DomainException("Pedido não pode ser confirmado sem itens.");

            if (Status == OrderStatus.Confirmed)
                throw new DomainException("Pedido já confirmado.");

            if (Status == OrderStatus.Cancelled)
                throw new DomainException("Pedido cancelado não pode ser confirmado.");

            Status = OrderStatus.Confirmed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Confirmed)
                throw new DomainException("Pedido confirmado não pode ser cancelado por esta operação.");

            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
