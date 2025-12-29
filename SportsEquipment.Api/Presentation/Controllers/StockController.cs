using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SportsEquipment.Application.DTOs.Stocks;
using SportsEquipment.Application.Commands.Stocks;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de estoque")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        /// <summary>
        /// Adiciona um estoque ao produto informado 
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [SwaggerOperation(Summary = "Adiciona estoque a um produto", Description = "Adiciona unidades ao estoque de um produto específico. Requer permissão de Administrador.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Estoque adicionado com sucesso", typeof(StockDto))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Dados inválidos ou produto não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão de administrador")]
        public async Task<IActionResult> AddStockAsync([FromBody] AddStockCommand command)
        {
            var stock = await _stockService.AddStockAsync(command);

            return Ok(stock);
        }

        /// <summary>
        /// Retorna o estoque de um produto pelo ID do produto
        /// </summary>
        [HttpGet("{productId:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Consulta estoque de produto", Description = "Retorna informações sobre o estoque de um produto específico.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Estoque encontrado", typeof(StockDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Produto ou estoque não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetStockAsync(Guid productId)
        {
            var stock = await _stockService.GetStockByProductIdAsync(productId);

            return Ok(stock);
        }
    }
}