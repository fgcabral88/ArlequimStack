using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;
using SportsEquipment.Application.DTOs.Orders;
using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Application.Messaging.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Orders;

namespace SportsEquipment.Tests.Unit.Application
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<IStockRepository> _mockStockRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IEventPublisher> _mockEventPublisher;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockStockRepo = new Mock<IStockRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockLogger = new Mock<ILogger<OrderService>>();

            _service = new OrderService(
                _mockOrderRepo.Object,
                _mockProductRepo.Object,
                _mockStockRepo.Object,
                _mockUnitOfWork.Object,
                _mockEventPublisher.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateOrder_Should_persist_order_and_decrease_stock_and_publish_event()
        {
            // Arrange
            var product = new Product("Football", "Size 5 ball", new Money(50m, "BRL"));
            var productId = product.Id;

            var stock = new ProductStock(productId);
            stock.AddStock(10, "NF-001");

            var createOrderCommand = new CreateOrderCommand
            {
                ClientDocument = "12345678900",
                SellerName = "Seller A",
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = productId, Quantity = 3 }
                }
            };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockStockRepo.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync(stock);
            _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockStockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductStock>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _mockEventPublisher.Setup(ep => ep.PublishAsync(It.IsAny<object>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateOrderAsync(createOrderCommand);

            // Assert repository + uow interactions
            _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _mockStockRepo.Verify(r => r.UpdateAsync(It.Is<ProductStock>(ps => ps.GetAvailableQuantity() == 7)), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);

            // Event published
            _mockEventPublisher.Verify(ep => ep.PublishAsync(It.IsAny<object>()), Times.Once);

            // Result assertions
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().ProductId.Should().Be(productId);
            result.Items.First().Quantity.Should().Be(3);
            result.TotalAmount.Should().Be(50m * 3);
            stock.GetAvailableQuantity().Should().Be(7);
        }

        [Fact]
        public async Task CreateOrder_When_EventPublisherThrows_OrderIsStillCreated_AndExceptionIsHandled()
        {
            // Arrange
            var product = new Product("Tennis Racket", "Pro racket", new Money(100m, "BRL"));
            var productId = product.Id;

            var stock = new ProductStock(productId);
            stock.AddStock(5, "NF-002");

            var createOrderCommand = new CreateOrderCommand
            {
                ClientDocument = "99999999999",
                SellerName = "Seller B",
                Items = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = productId, Quantity = 2 }
                }
            };

            _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockStockRepo.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync(stock);
            _mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockStockRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductStock>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Make event publisher throw
            _mockEventPublisher.Setup(ep => ep.PublishAsync(It.IsAny<object>())).ThrowsAsync(new Exception("publisher fail"));

            // Act
            var result = await _service.CreateOrderAsync(createOrderCommand);

            // Assert: order and stock updated despite publish failure
            _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            _mockStockRepo.Verify(r => r.UpdateAsync(It.Is<ProductStock>(ps => ps.GetAvailableQuantity() == 3)), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);

            // Event publisher should still be called, but error is caught
            _mockEventPublisher.Verify(ep => ep.PublishAsync(It.IsAny<object>()), Times.Once);

            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(200m);
            stock.GetAvailableQuantity().Should().Be(3);
        }

        [Fact]
        public async Task CreateOrder_WithMissingProduct_ThrowsDomainException()
        {
            // Arrange
            var pid = Guid.NewGuid();
            var cmd = new CreateOrderCommand
            {
                ClientDocument = "cli",
                SellerName = "seller",
                Items = new List<CreateOrderItemDto> {
                    new CreateOrderItemDto { ProductId = pid, Quantity = 1 }
                }
            };

            _mockProductRepo.Setup(p => p.GetByIdAsync(pid)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(cmd));

            _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_WithInsufficientStock_ThrowsDomainException()
        {
            // Arrange
            var product = new Product("X", "Y", new Money(10m, "BRL"));
            var pid = product.Id;

            _mockProductRepo.Setup(p => p.GetByIdAsync(pid)).ReturnsAsync(product);

            var stock = new ProductStock(pid);
            stock.AddStock(1, "NF-1");

            _mockStockRepo.Setup(s => s.GetByProductIdAsync(pid)).ReturnsAsync(stock);

            var cmd = new CreateOrderCommand
            {
                ClientDocument = "cli",
                SellerName = "seller",
                Items = new List<CreateOrderItemDto> { new CreateOrderItemDto { ProductId = pid, Quantity = 2 } }
            };

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.CreateOrderAsync(cmd));

            _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ThrowsDomainException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockOrderRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Order?)null);

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _service.GetByIdAsync(id));
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsDto()
        {
            // Arrange
            var order = new Order("123", "Seller");
            order.AddItem(Guid.NewGuid(), 2, new Money(20m, "BRL"));

            _mockOrderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);

            // Act
            var dto = await _service.GetByIdAsync(order.Id);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(order.Id);
            dto.TotalAmount.Should().Be(40m);
            dto.Items.Should().HaveCount(1);
        }
    }
}