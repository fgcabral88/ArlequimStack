namespace SportsEquipment.Application.Messaging.Interfaces
{
    /// <summary>
    /// Abstração leve para publicar eventos de domínio/integração.
    /// Implementações concretas vivem na infra (ex.: MassTransit).
    /// Mantém a camada application desacoplada de bibliotecas de mensageria.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    }
}
