using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.ValueObjects;

namespace SportsEquipment.Domain.Entities
{
    /// <summary>
    /// Produto do catálogo (equipamento esportivo).
    /// </summary>
    public class Product : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Money Price { get; private set; } = null!;
        public bool IsActive { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected Product() { }

        public Product(string name, string description, Money price)
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            IsActive = true;
            UpdatedAt = CreatedAt;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Nome do produto é obrigatório.");

            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPrice(Money price)
        {
            if (price == null)
                throw new DomainException("Preço é obrigatório.");

            if (price.Amount <= 0)
                throw new DomainException("Preço deve ser maior que zero.");

            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
