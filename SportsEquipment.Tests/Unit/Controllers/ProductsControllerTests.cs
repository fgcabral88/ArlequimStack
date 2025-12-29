using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SportsEquipment.Application.DTOs.Products;
using SportsEquipment.Api.Presentation.Controllers;
using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Tests.Unit.Controllers
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task CreateAsync_ReturnsCreatedResult()  
        {
            var mockService = new Mock<IProductService>();
            var dto = new ProductDto { Id = Guid.NewGuid(), Name = "Ball", Price = 10m, Currency = "BRL" };
            mockService.Setup(s => s.CreateAsync(It.IsAny<CreateProductCommand>())).ReturnsAsync(dto);

            var controller = new ProductsController(mockService.Object);

            var result = await controller.CreateAsync(new CreateProductCommand
            {
                Name = "Ball",
                Price = 10m,
                Currency = "BRL"
            });

            result.Should().BeOfType<CreatedAtActionResult>();
            var created = result as CreatedAtActionResult;
            created!.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOk_WhenFound()  
        {
            var id = Guid.NewGuid();
            var mockService = new Mock<IProductService>();
            var dto = new ProductDto { Id = id, Name = "Ball", Price = 10m, Currency = "BRL" };
            mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

            var controller = new ProductsController(mockService.Object);

            var result = await controller.GetByIdAsync(id);

            result.Should().BeOfType<OkObjectResult>();
            (result as OkObjectResult)!.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNoContent()  
        {
            var id = Guid.NewGuid();
            var mockService = new Mock<IProductService>();

            mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

            var controller = new ProductsController(mockService.Object);

            var result = await controller.DeleteAsync(id);

            result.Should().BeOfType<NoContentResult>();
        }
    }
}
