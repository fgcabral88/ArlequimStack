namespace SportsEquipment.Application.DTOs.Orders
{
    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
