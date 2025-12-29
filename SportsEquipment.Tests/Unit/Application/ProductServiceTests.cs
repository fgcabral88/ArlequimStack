using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Products;

namespace SportsEquipment.Tests.Unit.Application
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task CreateAsync_WithValidCommand_CreatesProduct()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ProductService>>();

            mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var service = new ProductService(mockRepo.Object, mockUow.Object, mockLogger.Object);
            var cmd = new CreateProductCommand { Name = "Ball", Description = "desc", Price = 10m, Currency = "BRL" };

            var result = await service.CreateAsync(cmd);

            result.Should().NotBeNull();
            result.Price.Should().Be(10m);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidPrice_Throws()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ProductService>>();

            var service = new ProductService(mockRepo.Object, mockUow.Object, mockLogger.Object);
            var cmd = new CreateProductCommand { Name = "Ball", Description = "desc", Price = 0m, Currency = "BRL" };

            await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(cmd));
        }

        [Fact]
        public async Task UpdateAsync_NotFound_Throws()
        {
            var mockRepo = new Mock<IProductRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ProductService>>();

            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

            var service = new ProductService(mockRepo.Object, mockUow.Object, mockLogger.Object);
            var cmd = new UpdateProductCommand { Id = Guid.NewGuid(), Name = "x", Price = 1m, Currency = "BRL", Description = "d" };

            await Assert.ThrowsAsync<DomainException>(() => service.UpdateAsync(cmd));
        }
    }
}