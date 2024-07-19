using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MokkilicoresExpressAPI.Models;
using System.Collections.Generic;
using System.Linq;
//logger
using Microsoft.Extensions.Logging;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InventarioController> _logger;
        private const string CacheKey = "Inventarios";

        public InventarioController(IMemoryCache cache, ILogger<InventarioController> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Inventario>> Get()
        {
            var inventarios = _cache.GetOrCreate(CacheKey, entry => new List<Inventario>());
            return Ok(inventarios);
        }

        [HttpGet("{id}")]
        public ActionResult<Inventario> Get(int id)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });
            return Ok(inventario);
        }

        // POST api/inventario - Crear un nuevo inventario
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public ActionResult Post([FromBody] Inventario inventario)
        {
            _logger.LogInformation($"Recibida solicitud POST para crear inventario: {System.Text.Json.JsonSerializer.Serialize(inventario)}");
            if (inventario == null)
            {
                return BadRequest(new { Message = "Inventario no puede ser nulo" });
            }

            if (string.IsNullOrEmpty(inventario.TipoLicor))
            {
                return BadRequest(new { Message = "Tipo de licor es un campo requerido" });
            }

            var inventarios = _cache.GetOrCreate(CacheKey, entry => new List<Inventario>());
            if (inventarios == null)
            {
                return StatusCode(500, new { Message = "Error al acceder a la caché" });
            }

            // Asignar ID y agregar a la lista
            inventario.Id = inventarios.Count > 0 ? inventarios.Max(i => i.Id) + 1 : 1;
            inventarios.Add(inventario);

            // Actualizar la caché
            _cache.Set(CacheKey, inventarios);

            // Log para depuración
            Console.WriteLine($"Inventario creado: {inventario.Id}, Tipo: {inventario.TipoLicor}");

            return CreatedAtAction(nameof(Get), new { id = inventario.Id }, inventario);
        }

        // PUT api/inventario/5 - Actualizar un inventario existente
        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Inventario updatedInventario)
        {
            if (updatedInventario == null)
                return BadRequest(new { Message = "Inventario no puede ser nulo" });

            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            if (string.IsNullOrEmpty(updatedInventario.TipoLicor))
                return BadRequest(new { Message = "Tipo de licor es un campo requerido" });

            inventario.CantidadEnExistencia = updatedInventario.CantidadEnExistencia;
            inventario.BodegaId = updatedInventario.BodegaId;
            inventario.FechaIngreso = updatedInventario.FechaIngreso;
            inventario.FechaVencimiento = updatedInventario.FechaVencimiento;
            inventario.TipoLicor = updatedInventario.TipoLicor;
            _cache.Set(CacheKey, inventarios);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var inventarios = _cache.Get<List<Inventario>>(CacheKey);
            var inventario = inventarios?.FirstOrDefault(i => i.Id == id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            inventarios.Remove(inventario);
            _cache.Set(CacheKey, inventarios);
            return NoContent();
        }
    }
}
