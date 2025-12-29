using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Application.DTOs.Orders;

namespace SportsEquipment.Application.Interfaces.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Cria e confirma um pedido (a implementação realiza validações de estoque e coordena a baixa).
        /// </summary>
        Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);

        Task<OrderDto> GetByIdAsync(Guid id);
    }
}
