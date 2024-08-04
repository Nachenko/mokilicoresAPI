using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MokkilicoresExpressAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(ApplicationDbContext context, ILogger<PedidoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> Get()
        {
            return await _context.Pedidos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> Get(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Pedido pedido)
        {
            _logger.LogDebug("Datos recibidos: {PedidoData}", JsonSerializer.Serialize(pedido));

            pedido.CostoTotal = pedido.CostoSinIVA * 1.13M;

            var cliente = await _context.Clientes.FindAsync(pedido.ClienteId);
            var inventario = await _context.Inventarios.FindAsync(pedido.InventarioId);
            var direccion = await _context.Direcciones.FindAsync(pedido.DireccionId);

            if (cliente == null || inventario == null || direccion == null || direccion.ClienteId != pedido.ClienteId)
            {
                return BadRequest("Cliente, Inventario, o Dirección no encontrados o no válidos.");
            }

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Pedido updatedPedido)
        {
            _logger.LogDebug("Datos recibidos para actualización: {PedidoData}", JsonSerializer.Serialize(updatedPedido));

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            pedido.ClienteId = updatedPedido.ClienteId;
            pedido.InventarioId = updatedPedido.InventarioId;
            pedido.Cantidad = updatedPedido.Cantidad;
            pedido.CostoSinIVA = updatedPedido.CostoSinIVA;
            pedido.CostoTotal = updatedPedido.CostoSinIVA * 1.13M;
            pedido.Estado = updatedPedido.Estado;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
