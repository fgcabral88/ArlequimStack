using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SportsEquipment.Application.DTOs.Orders;
using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Api.Presentation.Controllers;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Tests.Unit.Controllers
{
    public class OrdersControllerTests
    {
        [Fact]
        public async Task CreateOrderAsync_ReturnsCreated() 
        {
            var mockService = new Mock<IOrderService>();
            var dto = new OrderDto { Id = Guid.NewGuid(), ClientDocument = "C", SellerName = "S" };
            mockService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderCommand>())).ReturnsAsync(dto);

            var controller = new OrdersController(mockService.Object);

            var result = await controller.CreateOrderAsync(new CreateOrderCommand { ClientDocument = "C", SellerName = "S", Items = new System.Collections.Generic.List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 } } });

            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOk()  
        {
            var id = Guid.NewGuid();
            var mockService = new Mock<IOrderService>();
            var dto = new OrderDto { Id = id, ClientDocument = "C", SellerName = "S" };

            mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

            var controller = new OrdersController(mockService.Object);

            var result = await controller.GetByIdAsync(id);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}