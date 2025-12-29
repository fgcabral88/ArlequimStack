using SportsEquipment.Application.DTOs.Orders;

namespace SportsEquipment.Application.Commands.Orders
{
    public class CreateOrderCommand
    {
        public string ClientDocument { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}
