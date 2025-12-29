using MassTransit;
using Microsoft.EntityFrameworkCore;
using SportsEquipment.Messaging.Consumers;
using SportsEquipment.Infrastructure.Data;
using SportsEquipment.Infrastructure.Mapping;
using SportsEquipment.Infrastructure.Messaging;
using SportsEquipment.Infrastructure.Repositories;
using SportsEquipment.Api.Presentation.Security.Jwt;
using SportsEquipment.Application.Security.Password;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Security.Interfaces;
using SportsEquipment.Application.Messaging.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;
using SportsEquipment.Application.Services.Implementation.Users;
using SportsEquipment.Application.Services.Implementation.Orders;
using SportsEquipment.Application.Services.Implementation.Stocks;
using SportsEquipment.Application.Services.Implementation.Products;

namespace SportsEquipment.Api.Presentation.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 44)));
            });

            // AutoMapper 
            services.AddAutoMapper(configuration => configuration.AddProfile(new AutoMapperProfile()));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            // Application Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IOrderService, OrderService>();

            // Security
            services.AddSingleton<ITokenProvider, JwtTokenProvider>();
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

            // Messaging abstraction
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

            // MassTransit
            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitUri = configuration.GetValue<string>("RabbitMq:Uri") ?? "rabbitmq://rabbitmq";
                    cfg.Host(new Uri(rabbitUri), h =>
                    {
                        h.Username(configuration.GetValue<string>("RabbitMq:User") ?? "guest");
                        h.Password(configuration.GetValue<string>("RabbitMq:Password") ?? "guest");
                    });

                    cfg.ReceiveEndpoint("order-created-queue", ep =>
                    {
                        ep.ConfigureConsumer<OrderCreatedConsumer>(context);
                        ep.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
                    });
                });
            });

            return services;
        }
    }
}