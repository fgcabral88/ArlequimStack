using System.Text.Json;
using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Messaging.Events;
using SportsEquipment.Domain.ValueObjects;
using SportsEquipment.Application.DTOs.Orders;
using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Messaging.Interfaces;
using SportsEquipment.Application.Interfaces.Repositories;

namespace SportsEquipment.Application.Services.Implementation.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, IStockRepository stockRepository, IUnitOfWork unitOfWork, IEventPublisher eventPublisher, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _unitOfWork = unitOfWork;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand command)
        {
            _logger.LogInformation("Iniciando criação de pedido. Documento cliente: {ClientDocument}, Vendedor: {SellerName}", command.ClientDocument, command.SellerName);

            // Validação do command
            if (command == null)
            {
                _logger.LogError("Command de criação de pedido é nulo");
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(command.ClientDocument))
            {
                _logger.LogError("Documento do cliente é obrigatório");
                throw new DomainException("Documento do cliente é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(command.SellerName))
            {
                _logger.LogError("Nome do vendedor é obrigatório");
                throw new DomainException("Nome do vendedor é obrigatório.");
            }

            if (command.Items == null || !command.Items.Any())
            {
                _logger.LogError("Pedido precisa ter ao menos um item");
                throw new DomainException("Pedido precisa ter ao menos um item.");
            }

            _logger.LogDebug("Validando {ItemCount} itens no pedido", command.Items.Count);

            // Construir entidade Order
            var order = new Order(command.ClientDocument.Trim(), command.SellerName.Trim());

            // Carregar produtos e estoques necessários
            var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsById = new Dictionary<Guid, Product>();
            var stockByProductId = new Dictionary<Guid, ProductStock>();

            _logger.LogDebug("Carregando informações para {ProductCount} produtos: {ProductIds}",
                productIds.Count, string.Join(", ", productIds));

            foreach (var pid in productIds)
            {
                var product = await _productRepository.GetByIdAsync(pid);

                if (product == null)
                {
                    _logger.LogError("Produto {ProductId} não encontrado", pid);
                    throw new DomainException($"Produto {pid} não encontrado.");
                }
                productsById[pid] = product;

                var stock = await _stockRepository.GetByProductIdAsync(pid);

                if (stock == null)
                {
                    _logger.LogError("Estoque do produto {ProductId} não encontrado", pid);
                    throw new DomainException($"Estoque do produto {pid} não encontrado.");
                }
                stockByProductId[pid] = stock;

                _logger.LogDebug("Produto {ProductId} carregado. Estoque disponível: {StockQuantity}",
                    pid, stock.GetAvailableQuantity());
            }

            // Adicionar itens ao pedido usando preço atual do produto
            foreach (var item in command.Items)
            {
                var prod = productsById[item.ProductId];
                var unitPrice = new Money(prod.Price.Amount, prod.Price.Currency);

                order.AddItem(item.ProductId, item.Quantity, unitPrice);

                _logger.LogDebug("Item adicionado ao pedido: Produto {ProductId}, Quantidade {Quantity}, Preço {UnitPrice}",
                    item.ProductId, item.Quantity, unitPrice.Amount);
            }

            // Validar disponibilidade usando provider que consulta stockByProductId
            order.ValidateAvailability(pid =>
            {
                var stock = stockByProductId.ContainsKey(pid) ? stockByProductId[pid] : null;
                return stock?.GetAvailableQuantity() ?? 0;
            });

            _logger.LogInformation("Validação de estoque concluída para pedido com {ItemCount} itens", order.Items.Count);

            try
            {
                // Iniciar transação atômica
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 1. Persistir pedido
                    await _orderRepository.AddAsync(order);
                    _logger.LogInformation("Pedido {OrderId} persistido no repositório", order.Id);

                    // 2. Atualizar estoque
                    foreach (var item in order.Items)
                    {
                        var stock = stockByProductId[item.ProductId];

                        if (stock == null)
                        {
                            _logger.LogError("Estoque do produto {ProductId} não encontrado durante confirmação", item.ProductId);
                            throw new DomainException($"Estoque do produto {item.ProductId} não encontrado (esperado durante confirmação).");
                        }

                        var estoqueAnterior = stock.GetAvailableQuantity();
                        stock.RemoveStock(item.Quantity);
                        await _stockRepository.UpdateAsync(stock);

                        _logger.LogInformation("Estoque atualizado: Produto {ProductId}, Quantidade removida: {Quantity}, Estoque anterior: {OldStock}, Estoque atual: {NewStock}",
                            item.ProductId, item.Quantity, estoqueAnterior, stock.GetAvailableQuantity());
                    }

                    // 3. Salvar mudanças e confirmar transação
                    await _unitOfWork.CommitAsync();
                    _logger.LogInformation("Transação confirmada para pedido {OrderId}", order.Id);

                    // 4. Publicar evento APÓS transação confirmada (para evitar mensagens vazias)
                    await PublishOrderCreatedEvent(order);

                    // 5. Mapear para DTO e retornar
                    var orderDto = MapToDto(order);

                    _logger.LogInformation("Pedido {OrderId} criado com sucesso. Total: {TotalAmount}, Cliente: {ClientDocument}",
                        order.Id, orderDto.TotalAmount, orderDto.ClientDocument);

                    return orderDto;
                }
                catch (Exception)
                {
                    // Rollback em caso de erro
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido. Erro: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task PublishOrderCreatedEvent(Order order)
        {
            try
            {
                // Validar que a ordem existe antes de criar o evento
                if (order == null)
                {
                    _logger.LogError("Tentativa de publicar evento para pedido nulo");
                    return;
                }

                // Criar evento
                var evt = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    ClientDocument = order.ClientDocument,
                    SellerName = order.SellerName,
                    Total = order.Items.Sum(i => i.UnitPrice.Amount * i.Quantity),
                    Items = order.Items.Select(i => new OrderItemEvent
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice.Amount
                    }).ToList()
                };

                // VALIDAÇÃO CRÍTICA: Garantir que o evento não está vazio
                ValidateEvent(evt);

                // Serializar para verificar se gera JSON válido (apenas para debug)
                try
                {
                    var json = JsonSerializer.Serialize(evt);
                    _logger.LogDebug("JSON do evento para pedido {OrderId}: {Json}", order.Id, json);

                    if (string.IsNullOrWhiteSpace(json) || json == "{}")
                    {
                        _logger.LogError("Evento serializado está vazio ou inválido para pedido {OrderId}", order.Id);
                        throw new InvalidOperationException("Evento serializado está vazio");
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Erro ao serializar evento para pedido {OrderId}. Erro: {ErrorMessage}",
                        order.Id, jsonEx.Message);
                    throw;
                }

                // Publicar evento
                await _eventPublisher.PublishAsync(evt);

                _logger.LogInformation("Evento OrderCreated publicado com sucesso para pedido {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento OrderCreated para pedido {OrderId}. Erro: {ErrorMessage}",
                    order.Id, ex.Message);
                // NÃO lançar exceção - o pedido já foi criado com sucesso
            }
        }

        private void ValidateEvent(OrderCreatedEvent evt)
        {
            if (evt == null)
            {
                throw new ArgumentNullException(nameof(evt), "Evento não pode ser nulo");
            }

            if (evt.OrderId == Guid.Empty)
            {
                throw new ArgumentException("OrderId não pode ser vazio", nameof(evt));
            }

            if (string.IsNullOrWhiteSpace(evt.ClientDocument))
            {
                throw new ArgumentException("ClientDocument não pode ser vazio", nameof(evt));
            }

            if (string.IsNullOrWhiteSpace(evt.SellerName))
            {
                throw new ArgumentException("SellerName não pode ser vazio", nameof(evt));
            }

            if (evt.Items == null || !evt.Items.Any())
            {
                throw new ArgumentException("Items não pode ser nulo ou vazio", nameof(evt));
            }

            // Validar cada item
            foreach (var item in evt.Items)
            {
                if (item.ProductId == Guid.Empty)
                {
                    throw new ArgumentException($"ProductId não pode ser vazio no item", nameof(evt));
                }

                if (item.Quantity <= 0)
                {
                    throw new ArgumentException($"Quantidade deve ser maior que zero no item {item.ProductId}", nameof(evt));
                }

                if (item.UnitPrice <= 0)
                {
                    throw new ArgumentException($"UnitPrice deve ser maior que zero no item {item.ProductId}", nameof(evt));
                }
            }

            if (evt.Total <= 0)
            {
                throw new ArgumentException("Total deve ser maior que zero", nameof(evt));
            }
        }

        public async Task<OrderDto> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Buscando pedido por ID: {OrderId}", id);

            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Pedido {OrderId} não encontrado", id);
                throw new DomainException("Pedido não encontrado.");
            }

            _logger.LogDebug("Pedido {OrderId} encontrado. Status: {Status}, Itens: {ItemCount}",
                order.Id, order.Status, order.Items.Count);

            return MapToDto(order);
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                ClientDocument = order.ClientDocument,
                SellerName = order.SellerName,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice.Amount
                }).ToList(),
                TotalAmount = order.Items.Sum(i => i.UnitPrice.Amount * i.Quantity),
                Status = order.Status.ToString()
            };
        }
    }
}