namespace SportsEquipment.Application.Commands.Product
{
    public class UpdateProductCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "BRL";
    }
}
