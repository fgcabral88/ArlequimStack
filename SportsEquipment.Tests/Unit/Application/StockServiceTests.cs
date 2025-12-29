using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;
using SportsEquipment.Application.Commands.Stocks;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Stocks;

namespace SportsEquipment.Tests.Unit.Application
{
    public class StockServiceTests
    {
        [Fact]
        public async Task AddStockAsync_WhenProductMissing_Throws()
        {
            var productId = Guid.NewGuid();
            var mockStockRepo = new Mock<IStockRepository>();
            var mockProductRepo = new Mock<IProductRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StockService>>();

            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            var service = new StockService(mockStockRepo.Object, mockProductRepo.Object, mockUow.Object, mockLogger.Object);

            var cmd = new AddStockCommand { ProductId = productId, Quantity = 5, FiscalNoteNumber = "NF-1" };

            await Assert.ThrowsAsync<DomainException>(() => service.AddStockAsync(cmd));
        }

        [Fact]
        public async Task AddStockAsync_CreateNewStock_WhenMissing()
        {
            var productId = Guid.NewGuid();
            var mockStockRepo = new Mock<IStockRepository>();
            var mockProductRepo = new Mock<IProductRepository>();
            var mockUow = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<StockService>>();

            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new Product("X", "D", new Money(1m, "BRL")));
            mockStockRepo.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync((ProductStock?)null);
            mockStockRepo.Setup(r => r.AddAsync(It.IsAny<ProductStock>())).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var service = new StockService(mockStockRepo.Object, mockProductRepo.Object, mockUow.Object, mockLogger.Object);

            var cmd = new AddStockCommand { ProductId = productId, Quantity = 5, FiscalNoteNumber = "NF-1" };
            var result = await service.AddStockAsync(cmd);

            result.CurrentQuantity.Should().Be(5);
            mockStockRepo.Verify(r => r.AddAsync(It.IsAny<ProductStock>()), Times.Once);
        }
    }
}