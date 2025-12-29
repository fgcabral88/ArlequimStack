using AutoMapper;
using SportsEquipment.Application.DTOs.Orders;
using SportsEquipment.Application.DTOs.Products;
using SportsEquipment.Application.DTOs.Users;
using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Infrastructure.Mapping
{
    /// <summary>
    /// Perfis AutoMapper para mapear Domain <-> DTOs.
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User
            CreateMap<User, UserDto>();

            // Product => ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

            // ProductDto -> Product
            // Order -> OrderDto
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Sum(i => i.UnitPrice.Amount * i.Quantity) : 0m))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.UnitPrice, opt => opt.MapFrom(s => s.UnitPrice.Amount));
        }
    }
}
