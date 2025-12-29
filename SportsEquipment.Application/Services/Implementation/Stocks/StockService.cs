using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Application.DTOs.Stocks;
using SportsEquipment.Application.Commands.Stocks;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Interfaces.Repositories;

namespace SportsEquipment.Application.Services.Implementation.Stocks
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockService> _logger;

        public StockService(IStockRepository stockRepository, IProductRepository productRepository, IUnitOfWork unitOfWork, ILogger<StockService> logger)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<StockDto> AddStockAsync(AddStockCommand command)
        {
            _logger.LogInformation("Iniciando adição de estoque. Produto ID: {ProductId}, Quantidade: {Quantity}, Nota Fiscal: {FiscalNoteNumber}", command.ProductId, command.Quantity, command.FiscalNoteNumber);

            if (command is null)
            {
                _logger.LogError("Command de adição de estoque é nulo");

                throw new ArgumentNullException(nameof(command));
            }

            if (command.Quantity <= 0)
            {
                _logger.LogError("Quantidade inválida: {Quantity}. Deve ser maior que zero", command.Quantity);

                throw new DomainException("Quantidade deve ser maior que zero.");
            }

            var product = await _productRepository.GetByIdAsync(command.ProductId);

            if (product is null)
            {
                _logger.LogWarning("Produto não encontrado para adicionar estoque. Produto ID: {ProductId}", command.ProductId);

                throw new DomainException("Produto não encontrado.");
            }

            _logger.LogDebug("Produto encontrado. Nome: {ProductName}, Preço: {Price}", product.Name, product.Price.Amount);

            // Verifica se já existe um registro de estoque
            var stock = await _stockRepository.GetByProductIdAsync(command.ProductId);

            await _unitOfWork.BeginTransactionAsync();

            _logger.LogDebug("Transação de banco de dados iniciada para adição de estoque");

            try
            {
                if (stock is null)
                {
                    _logger.LogDebug("Criando novo registro de estoque para o produto {ProductId}", command.ProductId);

                    stock = new ProductStock(command.ProductId);
                    stock.AddStock(command.Quantity, command.FiscalNoteNumber);

                    await _stockRepository.AddAsync(stock);

                    _logger.LogInformation("Novo registro de estoque criado para produto {ProductId}", command.ProductId);
                }
                else
                {
                    var estoqueAnterior = stock.GetAvailableQuantity();

                    stock.AddStock(command.Quantity, command.FiscalNoteNumber);

                    await _stockRepository.UpdateAsync(stock);

                    _logger.LogInformation("Estoque atualizado. Produto {ProductId}, Quantidade adicionada: {Quantity}, Estoque anterior: {OldStock}, Estoque atual: {NewStock}", command.ProductId, command.Quantity, estoqueAnterior, stock.GetAvailableQuantity());
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Estoque adicionado com sucesso. Produto ID: {ProductId}, Quantidade total: {CurrentQuantity}", command.ProductId, stock.GetAvailableQuantity());

                return new StockDto
                {
                    ProductId = stock.ProductId,
                    CurrentQuantity = stock.GetAvailableQuantity()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante adição de estoque. Realizando rollback. Erro: {ErrorMessage}", ex.Message);

                await _unitOfWork.RollbackAsync();

                _logger.LogWarning("Rollback da transação realizado devido a erro na adição de estoque");

                throw;
            }
        }

        public async Task<StockDto> GetStockByProductIdAsync(Guid productId)
        {
            _logger.LogDebug("Consultando estoque para produto ID: {ProductId}", productId);

            var stock = await _stockRepository.GetByProductIdAsync(productId);

            if (stock is null)
            {
                _logger.LogDebug("Nenhum registro de estoque encontrado para produto ID: {ProductId}. Retornando quantidade zero.", productId);

                return new StockDto
                {
                    ProductId = productId,
                    CurrentQuantity = 0
                };
            }

            var currentQuantity = stock.GetAvailableQuantity();

            _logger.LogDebug("Estoque encontrado. Produto ID: {ProductId}, Quantidade disponível: {CurrentQuantity}", productId, currentQuantity);

            return new StockDto
            {
                ProductId = stock.ProductId,
                CurrentQuantity = currentQuantity
            };
        }
    }
}