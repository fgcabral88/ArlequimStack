namespace SportsEquipment.Messaging.Events
{
    public class OrderItemEvent
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
