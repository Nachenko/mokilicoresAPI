using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MokkilicoresExpressAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DireccionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Direccion>>> Get()
        {
            return await _context.Direcciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Direccion>> Get(int id)
        {
            var direccion = await _context.Direcciones.FindAsync(id);
            if (direccion == null)
                return NotFound();
            return Ok(direccion);
        }

        [HttpGet("Usuario/{identificacion}")]
        public async Task<ActionResult<IEnumerable<Direccion>>> GetDireccionesByUsuario(string identificacion)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Identificacion == identificacion);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado");
            }

            var direccionesCliente = await _context.Direcciones
                .Where(d => d.ClienteId == cliente.Id)
                .ToListAsync();

            return Ok(direccionesCliente);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Direccion direccion)
        {
            if (direccion == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de dirección no válidos.");
            }
            _context.Direcciones.Add(direccion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = direccion.Id }, direccion);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Direccion updatedDireccion)
        {
            if (updatedDireccion == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de dirección no válidos.");
            }
            var direccion = await _context.Direcciones.FindAsync(id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            direccion.ClienteId = updatedDireccion.ClienteId;
            direccion.Provincia = updatedDireccion.Provincia;
            direccion.Canton = updatedDireccion.Canton;
            direccion.Distrito = updatedDireccion.Distrito;
            direccion.PuntoEnWaze = updatedDireccion.PuntoEnWaze;
            direccion.EsCondominio = updatedDireccion.EsCondominio;
            direccion.EsPrincipal = updatedDireccion.EsPrincipal;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var direccion = await _context.Direcciones.FindAsync(id);
            if (direccion == null)
                return NotFound(new { Message = "Dirección no encontrada." });

            _context.Direcciones.Remove(direccion);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Dirección eliminada correctamente." });
        }

        [HttpGet("Cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Direccion>>> GetDireccionesPorCliente(int clienteId)
        {
            var direcciones = await _context.Direcciones
                .Where(d => d.ClienteId == clienteId)
                .ToListAsync();
            return Ok(direcciones);
        }
    }
}
