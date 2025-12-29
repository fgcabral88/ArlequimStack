namespace SportsEquipment.Application.DTOs.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string ClientDocument { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
