using SportsEquipment.Application.Commands.Stocks;
using SportsEquipment.Application.DTOs.Stocks;

namespace SportsEquipment.Application.Interfaces.Services
{
    public interface IStockService
    {
        Task<StockDto> AddStockAsync(AddStockCommand command);
        Task<StockDto> GetStockByProductIdAsync(Guid productId);
    }
}
