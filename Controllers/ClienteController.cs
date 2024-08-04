using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MokkilicoresExpressAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> Get()
        {
            return await _context.Clientes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> Get(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();
            return Ok(cliente);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Cliente cliente)
        {
            if (cliente == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de cliente no válidos.");
            }
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = cliente.Id }, cliente);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Cliente updatedCliente)
        {
            if (updatedCliente == null || !ModelState.IsValid)
            {
                return BadRequest("Datos de cliente no válidos.");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

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

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();
            if (!User.IsInRole("Admin"))
            {
                return Unauthorized();
            }
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
