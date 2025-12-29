namespace SportsEquipment.Application.Commands.Product
{
    public class CreateProductCommand
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "BRL";
    }
}
