using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.DTOs.Products;

namespace SportsEquipment.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(CreateProductCommand command);
        Task<ProductDto> UpdateAsync(UpdateProductCommand command);
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task DeleteAsync(Guid id);
    }
}
