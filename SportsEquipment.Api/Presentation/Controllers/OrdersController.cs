using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using SportsEquipment.Application.DTOs.Orders;
using SportsEquipment.Application.Commands.Orders;
using SportsEquipment.Application.Interfaces.Services;

namespace SportsEquipment.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Gerenciamento de pedidos")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Cria um pedido (Autenticado como Vendedor)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Seller")]
        [SwaggerOperation(Summary = "Cria um novo pedido", Description = "Cria um pedido com os itens especificados. Requer autenticação como Vendedor.")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Pedido criado com sucesso", typeof(OrderDto))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Requisição inválida")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, "Sem permissão (não é vendedor)")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderCommand command)
        {
            var order = await _orderService.CreateOrderAsync(command);

            return CreatedAtAction("GetById", new { id = order.Id }, order);
        }

        /// <summary>
        /// Retorna um pedido por id (Autenticado)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Obtém um pedido por Id", Description = "Retorna os detalhes de um pedido específico.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Pedido encontrado", typeof(OrderDto))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Pedido não encontrado")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Não autenticado")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);

            return Ok(order);
        }
    }
}