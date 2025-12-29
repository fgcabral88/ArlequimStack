using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SportsEquipment.Application.DTOs.Products;
using SportsEquipment.Application.Commands.Product;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de produtos")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Cria um produto (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [SwaggerOperation(Summary = "Cria um novo produto", Description = "Adiciona um novo produto ao catálogo. Requer permissão de Administrador.")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Produto criado com sucesso", typeof(ProductDto))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Dados inválidos")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão de administrador")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProductCommand command)
        {
            var product = await _productService.CreateAsync(command);

            return CreatedAtAction("GetById", new { id = product.Id }, product);
        }

        /// <summary>
        /// Atualiza um produto (Admin only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrator")]
        [SwaggerOperation(Summary = "Atualiza um produto existente", Description = "Atualiza as informações de um produto específico. Requer permissão de Administrador.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Produto atualizado com sucesso", typeof(ProductDto))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "ID não corresponde ou dados inválidos")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Produto não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão de administrador")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest("Id mismatch.");

            var updated = await _productService.UpdateAsync(command);

            return Ok(updated);
        }

        /// <summary>
        /// Retorna um produto por id (Autenticado)
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Obtém um produto por Id", Description = "Retorna os detalhes de um produto específico.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Produto encontrado", typeof(ProductDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Produto não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);

            return Ok(product);
        }

        /// <summary>
        /// Lista todos os produtos (Autenticado)
        /// </summary>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Lista todos os produtos", Description = "Retorna uma lista com todos os produtos disponíveis.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Lista de produtos retornada", typeof(List<ProductDto>))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _productService.GetAllAsync();

            return Ok(list);
        }

        /// <summary>
        /// Deleta um produto (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Administrator")]
        [SwaggerOperation(Summary = "Remove um produto", Description = "Remove permanentemente um produto do sistema. Requer permissão de Administrador.")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "Produto removido com sucesso")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Produto não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão de administrador")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }
    }
}