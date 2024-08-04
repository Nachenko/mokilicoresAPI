using Microsoft.AspNetCore.Authorization;
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
    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventario>>> Get()
        {
            return await _context.Inventarios.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventario>> Get(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });
            return Ok(inventario);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Inventario inventario)
        {
            if (inventario == null || !ModelState.IsValid)
            {
                return BadRequest(new { Message = "Inventario no puede ser nulo" });
            }

            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = inventario.Id }, inventario);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Inventario updatedInventario)
        {
            if (updatedInventario == null)
                return BadRequest(new { Message = "Inventario no puede ser nulo" });

            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            inventario.CantidadEnExistencia = updatedInventario.CantidadEnExistencia;
            inventario.BodegaId = updatedInventario.BodegaId;
            inventario.FechaIngreso = updatedInventario.FechaIngreso;
            inventario.FechaVencimiento = updatedInventario.FechaVencimiento;
            inventario.TipoLicor = updatedInventario.TipoLicor;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);
            if (inventario == null)
                return NotFound(new { Message = $"Inventario con ID {id} no encontrado" });

            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
