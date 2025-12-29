using MassTransit;
using SportsEquipment.Application.Messaging.Interfaces;

namespace SportsEquipment.Infrastructure.Messaging
{
    /// <summary>
    /// Implementação concreta de IEventPublisher usando MassTransit (RabbitMQ).
    /// </summary>
    public class MassTransitEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            return _publishEndpoint.Publish(@event);
        }
    }
}
