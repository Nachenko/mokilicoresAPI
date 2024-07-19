using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<PedidoController> _logger;
        private const string CacheKey = "Pedidos";
        private const string ClientesCacheKey = "Clientes";
        private const string InventariosCacheKey = "Inventarios";
        private const string DireccionesCacheKey = "Direcciones";

        public PedidoController(IMemoryCache cache, ILogger<PedidoController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Pedido>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Pedido> pedidos))
            {
                pedidos = new List<Pedido>();
                _cache.Set(CacheKey, pedidos);
            }

            _logger.LogDebug("Get Pedidos llamado. Pedidos: {pedidos}", pedidos);
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public ActionResult<Pedido> Get(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            _logger.LogDebug("Get Pedido llamado con id {PedidoId}. Pedido: {pedido}", id, pedido);
            return Ok(pedido);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Pedido pedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos: {PedidoData}", JsonSerializer.Serialize(pedido));

            var pedidos = _cache.Get<List<Pedido>>(CacheKey) ?? new List<Pedido>();
            pedido.Id = pedidos.Count > 0 ? pedidos.Max(p => p.Id) + 1 : 1;
            pedido.CostoTotal = pedido.CostoSinIVA * 1.13M; // Calcular el costo total

            // Load related data
            var clientes = _cache.Get<List<Cliente>>(ClientesCacheKey) ?? new List<Cliente>();
            var inventarios = _cache.Get<List<Inventario>>(InventariosCacheKey) ?? new List<Inventario>();
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey) ?? new List<Direccion>();
            

            var cliente = clientes.FirstOrDefault(c => c.Id == pedido.ClienteId);
            var inventario = inventarios.FirstOrDefault(i => i.Id == pedido.InventarioId);
            var direccion = direcciones.FirstOrDefault(d => d.Id == pedido.DireccionId && d.ClienteId == pedido.ClienteId);


            if (inventario == null)
            {
                _logger.LogError("Error: Cliente o Inventario no encontrados.");
                return BadRequest("Cliente o Inventario no encontrados.");
            }
            if (direccion == null)
            {
                _logger.LogError("Error: Dirección no encontrada o no pertenece al cliente.");
                return BadRequest("Dirección no válida para este cliente.");
            }
            if (cliente == null)
            {
                _logger.LogError("Error: Cliente no encontrado.");
                return BadRequest("Cliente no encontrado.");
            }


            pedidos.Add(pedido);
            _logger.LogInformation("Pedido creado exitosamente con id {PedidoId}.", pedido.Id);
            _cache.Set(CacheKey, pedidos);
            return CreatedAtAction(nameof(Get), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Pedido updatedPedido)
        {
            // Logging para ver los datos recibidos
            _logger.LogDebug("Datos recibidos para actualización: {PedidoData}", JsonSerializer.Serialize(updatedPedido));

            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            pedido.ClienteId = updatedPedido.ClienteId;
            pedido.InventarioId = updatedPedido.InventarioId;
            pedido.Cantidad = updatedPedido.Cantidad;
            pedido.CostoSinIVA = updatedPedido.CostoSinIVA;
            pedido.CostoTotal = updatedPedido.CostoSinIVA * 1.13M; // Calcular el costo total
            pedido.Estado = updatedPedido.Estado;

            // Load related data
            var clientes = _cache.Get<List<Cliente>>(ClientesCacheKey) ?? new List<Cliente>();
            var inventarios = _cache.Get<List<Inventario>>(InventariosCacheKey) ?? new List<Inventario>();

            var cliente = clientes.FirstOrDefault(c => c.Id == pedido.ClienteId);
            var inventario = inventarios.FirstOrDefault(i => i.Id == pedido.InventarioId);

            if (cliente == null || inventario == null)
            {
                _logger.LogError("Error: Cliente o Inventario no encontrados.");
                return BadRequest("Cliente o Inventario no encontrados.");
            }

            _logger.LogInformation("Pedido actualizado exitosamente con id {PedidoId}.", id);
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var pedidos = _cache.Get<List<Pedido>>(CacheKey);
            var pedido = pedidos?.FirstOrDefault(p => p.Id == id);
            if (pedido == null)
            {
                _logger.LogWarning("Pedido with id {PedidoId} not found.", id);
                return NotFound();
            }

            pedidos.Remove(pedido);
            _logger.LogInformation("Pedido eliminado exitosamente con id {PedidoId}.", id);
            _cache.Set(CacheKey, pedidos);
            return NoContent();
        }
    }
}
