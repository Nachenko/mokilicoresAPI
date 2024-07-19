using Microsoft.AspNetCore.Mvc;
using MokkilicoresExpressAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private const string ClientesCacheKey = "Clientes";
        private const string DireccionesCacheKey = "Direcciones";


        public DireccionController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Direccion>> Get()
        {
            if (!_cache.TryGetValue(DireccionesCacheKey, out List<Direccion> direcciones))
            {
                direcciones = new List<Direccion>();
                _cache.Set(DireccionesCacheKey, direcciones);
            }
            return Ok(direcciones);
        }

        [HttpGet("{id}")]
        public ActionResult<Direccion> Get(int id)
        {
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound();
            return Ok(direccion);
        }

        [HttpGet("Usuario/{identificacion}")]
        public ActionResult<IEnumerable<Direccion>> GetDireccionesByUsuario(string identificacion)
        {
            var clientes = _cache.Get<List<Cliente>>(ClientesCacheKey);
            var cliente = clientes?.FirstOrDefault(c => c.Identificacion == identificacion);
            
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado");
            }

            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey);
            var direccionesCliente = direcciones?
                .Where(d => d.ClienteId == cliente.Id)
                .Select(d => new Direccion
                {
                    Id = d.Id,
                    ClienteId = d.ClienteId,
                    Provincia = d.Provincia,
                    Canton = d.Canton,
                    Distrito = d.Distrito,
                    PuntoEnWaze = d.PuntoEnWaze,
                    EsCondominio = d.EsCondominio,
                    EsPrincipal = d.EsPrincipal,
                    DireccionCompleta = $"{d.Id} - {d.Provincia}, {d.Canton}, {d.Distrito}"
                })
                .ToList();

            return Ok(direccionesCliente);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Direccion direccion)
        {
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey) ?? new List<Direccion>();
            direccion.Id = direcciones.Count > 0 ? direcciones.Max(d => d.Id) + 1 : 1;
            direcciones.Add(direccion);
            _cache.Set(DireccionesCacheKey, direcciones);
            return CreatedAtAction(nameof(Get), new { id = direccion.Id }, direccion);
        }

        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Direccion updatedDireccion)
        {
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            direccion.ClienteId = updatedDireccion.ClienteId;
            direccion.Provincia = updatedDireccion.Provincia;
            direccion.Canton = updatedDireccion.Canton;
            direccion.Distrito = updatedDireccion.Distrito;
            direccion.PuntoEnWaze = updatedDireccion.PuntoEnWaze;
            direccion.EsCondominio = updatedDireccion.EsCondominio;
            direccion.EsPrincipal = updatedDireccion.EsPrincipal;
            _cache.Set(DireccionesCacheKey, direcciones);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey);
            var direccion = direcciones?.FirstOrDefault(d => d.Id == id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            direcciones.Remove(direccion);
            _cache.Set(DireccionesCacheKey, direcciones);
            return Ok(new { Message = "Dirección eliminada correctamente." });
        }

        [HttpGet("Cliente/{clienteId}")]
        public ActionResult<IEnumerable<Direccion>> GetDireccionesPorCliente(int clienteId)
        {
            var direcciones = _cache.Get<List<Direccion>>(DireccionesCacheKey)?.Where(d => d.ClienteId == clienteId).ToList();
            return Ok(direcciones);
        }
    }
}
