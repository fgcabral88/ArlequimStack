using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportsEquipment.Infrastructure.Data;
using SportsEquipment.Infrastructure.Mapping;
using SportsEquipment.Application.DTOs.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportsEquipment.Infrastructure.Repositories;
using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Application.Commands.Stocks;
using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Messaging.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Orders;
using SportsEquipment.Application.Services.Implementation.Stocks;
using SportsEquipment.Application.Services.Implementation.Products;

namespace SportsEquipment.Tests.Integration
{
    public class EndToEndIntegrationTests
    {
        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseInMemoryDatabase(databaseName: $"db_{Guid.NewGuid()}");
                opts.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddAutoMapper(cfg => cfg.AddProfile(new AutoMapperProfile()));

            // Repositories (real)
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Simple EventPublisher stub to capture publish calls
            var pubMock = new Mock<IEventPublisher>();

            pubMock.Setup(p => p.PublishAsync(It.IsAny<object>())).Returns(Task.CompletedTask);
            services.AddSingleton(pubMock.Object);

            // Services (real)
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IOrderService, OrderService>();

            // minimal logger mocks
            services.AddLogging();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task FullFlow_CreateProduct_AddStock_CreateOrder_Succeeds()
        {
            var sp = BuildServiceProvider();

            var productSvc = sp.GetRequiredService<IProductService>();
            var stockSvc = sp.GetRequiredService<IStockService>();
            var orderSvc = sp.GetRequiredService<IOrderService>();
            var db = sp.GetRequiredService<ApplicationDbContext>();

            // Create product
            var createProduct = new CreateProductCommand { Name = "Ball", Description = "desc", Price = 10m, Currency = "BRL" };
            var p = await productSvc.CreateAsync(createProduct);

            p.Should().NotBeNull();
            p.Price.Should().Be(10m);

            // Add stock
            var addStock = new AddStockCommand { ProductId = p.Id, Quantity = 5, FiscalNoteNumber = "NF-1" };
            var stock = await stockSvc.AddStockAsync(addStock);

            stock.CurrentQuantity.Should().Be(5);

            // Create order
            var cmd = new CreateOrderCommand
            {
                ClientDocument = "123",
                SellerName = "Seller",
                Items = new System.Collections.Generic.List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = p.Id, Quantity = 2 }
                }
            };

            var order = await orderSvc.CreateOrderAsync(cmd);

            order.Should().NotBeNull();
            order.Items.Should().HaveCount(1);
            order.TotalAmount.Should().Be(20m);

            var ps = db.ProductStocks.SingleOrDefault(x => x.ProductId == p.Id);

            ps.Should().NotBeNull();
            ps!.GetAvailableQuantity().Should().Be(3);
        }
    }
}
