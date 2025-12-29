using MassTransit;
using Microsoft.Extensions.Logging;
using SportsEquipment.Messaging.Events;

namespace SportsEquipment.Messaging.Consumers
{
    /// <summary>
    /// Consumer de exemplo que recebe OrderCreatedEvent.
    /// Pode salvar em um read-model, enviar e-mail, integrar com ERP etc.
    /// </summary>
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation("Received OrderCreatedEvent. OrderId={OrderId} Total={Total}", evt.OrderId, evt.Total);

            return Task.CompletedTask;
        }
    }
}
