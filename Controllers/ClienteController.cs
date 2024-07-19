using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string CacheKey = "Clientes";

        public ClienteController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Cliente>> Get()
        {
            if (!_cache.TryGetValue(CacheKey, out List<Cliente> clientes))
            {
                clientes = new List<Cliente>();
                _cache.Set(CacheKey, clientes);
            }
            return Ok(clientes);
        }

        

        [HttpGet("{id}")]
        public ActionResult<Cliente> Get(int id)
        {
            var clientes = _cache.Get<List<Cliente>>(CacheKey);
            var cliente = clientes?.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
                return NotFound();
            return Ok(cliente);
        }

        

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Post([FromBody] Cliente cliente)
        {
            if (cliente == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de cliente no válidos.");
            }
            var clientes = _cache.Get<List<Cliente>>(CacheKey);
            cliente.Id = clientes.Count > 0 ? clientes.Max(c => c.Id) + 1 : 1;
            clientes.Add(cliente);
            _cache.Set(CacheKey, clientes);
            return CreatedAtAction(nameof(Get), new { id = cliente.Id }, cliente);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Cliente updatedCliente)
        {
            if (updatedCliente == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de cliente no válidos.");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var clientes = _cache.Get<List<Cliente>>(CacheKey);
            var cliente = clientes?.FirstOrDefault(c => c.Id == id);
            
            if (cliente == null)
                return NotFound();
            
            // Si el usuario no es admin, solo puede editar su propio perfil
            if (!User.IsInRole("Admin") && cliente.Identificacion != userId)
                return Unauthorized();

            cliente.Nombre = updatedCliente.Nombre;
            cliente.Apellido = updatedCliente.Apellido;
            cliente.Provincia = updatedCliente.Provincia;
            cliente.Canton = updatedCliente.Canton;
            cliente.Distrito = updatedCliente.Distrito;
            cliente.DineroCompradoTotal = updatedCliente.DineroCompradoTotal;
            cliente.DineroCompradoUltimoAno = updatedCliente.DineroCompradoUltimoAno;
            cliente.DineroCompradoUltimos6Meses = updatedCliente.DineroCompradoUltimos6Meses;
            _cache.Set(CacheKey, clientes);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var clientes = _cache.Get<List<Cliente>>(CacheKey);
            var cliente = clientes?.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
                return NotFound();
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized();
            }
            clientes.Remove(cliente);
            _cache.Set(CacheKey, clientes);
            return NoContent();
        }

    }
}
