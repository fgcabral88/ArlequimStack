using Microsoft.Extensions.Logging;
using SportsEquipment.Domain.Common;
using SportsEquipment.Domain.Entities;
using SportsEquipment.Domain.ValueObjects;
using SportsEquipment.Application.DTOs.Products;
using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.Interfaces.Services;
using SportsEquipment.Application.Interfaces.Repositories;

namespace SportsEquipment.Application.Services.Implementation.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ProductDto> CreateAsync(CreateProductCommand command)
        {
            _logger.LogInformation("Iniciando criação de produto. Nome: {ProductName}, Preço: {Price} {Currency}", command.Name, command.Price, command.Currency);

            if (command is null)
            {
                _logger.LogError("Command de criação de produto é nulo");
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                _logger.LogError("Nome do produto não fornecido");
                throw new DomainException("Nome do produto é obrigatório.");
            }

            if (command.Price <= 0)
            {
                _logger.LogError("Preço inválido: {Price}. Deve ser maior que zero", command.Price);
                throw new DomainException("Preço deve ser maior que zero.");
            }

            var money = new Money(command.Price, command.Currency);
            var product = new Product(command.Name.Trim(), command.Description?.Trim() ?? string.Empty, money);

            _logger.LogDebug("Produto criado em memória. ID: {ProductId}, Nome: {ProductName}", product.Id, product.Name);

            try
            {
                await _productRepository.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Produto criado com sucesso. ID: {ProductId}, Nome: {ProductName}, Preço: {Price} {Currency}", product.Id, product.Name, product.Price.Amount, product.Price.Currency);

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante criação do produto. Erro: {ErrorMessage}", ex.Message);

                throw;
            }
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductCommand command)
        {
            _logger.LogInformation("Iniciando atualização do produto. ID: {ProductId}, Novo nome: {ProductName}, Novo preço: {Price} {Currency}", command.Id, command.Name, command.Price, command.Currency);

            if (command is null)
            {
                _logger.LogError("Command de atualização de produto é nulo");

                throw new ArgumentNullException(nameof(command));
            }

            var existing = await _productRepository.GetByIdAsync(command.Id);

            if (existing is null)
            {
                _logger.LogWarning("Produto não encontrado para atualização. ID: {ProductId}", command.Id);

                throw new DomainException("Produto não encontrado.");
            }

            _logger.LogDebug("Produto encontrado para atualização. Nome anterior: {OldName}, Preço anterior: {OldPrice} {OldCurrency}", existing.Name, existing.Price.Amount, existing.Price.Currency);

            existing.SetName(command.Name);
            existing.SetDescription(command.Description ?? string.Empty);
            existing.SetPrice(new Money(command.Price, command.Currency));

            _logger.LogDebug("Produto atualizado em memória. Novo nome: {NewName}, Novo preço: {NewPrice} {NewCurrency}", existing.Name, existing.Price.Amount, existing.Price.Currency);

            try
            {
                await _productRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Produto atualizado com sucesso. ID: {ProductId}", existing.Id);

                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante atualização do produto. Erro: {ErrorMessage}", ex.Message);

                throw;
            }
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Buscando produto por ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                _logger.LogWarning("Produto não encontrado. ID: {ProductId}", id);

                throw new DomainException("Produto não encontrado.");
            }

            _logger.LogDebug("Produto encontrado. ID: {ProductId}, Nome: {ProductName}, Preço: {Price} {Currency}", product.Id, product.Name, product.Price.Amount, product.Price.Currency);

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            _logger.LogDebug("Buscando todos os produtos");

            var products = await _productRepository.GetAllAsync();

            _logger.LogInformation("Recuperados {ProductCount} produtos do banco de dados", products.Count());

            return products.Select(MapToDto);
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Iniciando exclusão do produto. ID: {ProductId}", id);

            var existing = await _productRepository.GetByIdAsync(id);

            if (existing is null)
            {
                _logger.LogWarning("Produto não encontrado para exclusão. ID: {ProductId}", id);
                throw new DomainException("Produto não encontrado.");
            }

            _logger.LogDebug("Produto encontrado para exclusão. Nome: {ProductName}, Preço: {Price} {Currency}", existing.Name, existing.Price.Amount, existing.Price.Currency);

            try
            {
                await _productRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Produto excluído com sucesso. ID: {ProductId}, Nome: {ProductName}", id, existing.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante exclusão do produto. Erro: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                IsActive = product.IsActive
            };
        }
    }
}