namespace SportsEquipment.Domain.Common
{
    /// <summary>
    /// Exceção para regras de negócio / validações do domínio.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException() { }

        public DomainException(string message) 
            : base(message) { }

        public DomainException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
