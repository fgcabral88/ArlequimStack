namespace SportsEquipment.Messaging.Events
{
    /// <summary>
    /// Evento publicado quando um pedido é confirmado com sucesso.
    /// Propriedades simples para consumo por outros microsserviços.
    /// </summary>
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public string ClientDocument { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public List<OrderItemEvent> Items { get; set; } = new List<OrderItemEvent>();
        public decimal Total { get; set; }
    }

}
