namespace SportsEquipment.Domain.ValueObjects
{
    /// <summary>
    /// Objeto de valor para representar montantes monetários.
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            Amount = decimal.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Multiply(int factor)
        {
            if (factor < 0)
                throw new ArgumentException("Factor cannot be negative.", nameof(factor));

            return new Money(Amount * factor, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (!string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot operate on Money with different currencies.");
        }

        public override bool Equals(object? obj) => Equals(obj as Money);  

        public bool Equals(Money? other)  
        {
            if (other is null) return false;  

            return Amount == other.Amount && string.Equals(Currency, other.Currency, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        public override string ToString() => $"{Currency} {Amount:N2}";

        public static bool operator ==(Money? left, Money? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Money? left, Money? right) => !(left == right);

        public static Money operator +(Money left, Money right) => left.Add(right);

        public static Money operator *(Money money, int factor) => money.Multiply(factor);

        public static Money operator *(int factor, Money money) => money.Multiply(factor);
    }
}