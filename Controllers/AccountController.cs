using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest.Identificacion == "admin" && loginRequest.Password == "admin")
            {
                return Ok(new LoginResponse { Token = _tokenService.GenerateJwtToken("admin", "Admin"), Identificacion = "admin", Role = "Admin" });
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Identificacion == loginRequest.Identificacion);
            if (cliente == null || !ValidatePassword(cliente, loginRequest.Password))
            {
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }

            return Ok(new LoginResponse { Token = _tokenService.GenerateJwtToken(cliente.Identificacion, "User"), Identificacion = cliente.Identificacion, Role = "User" });
        }

        private bool ValidatePassword(Cliente cliente, string password)
        {
            var expectedPassword = $"{cliente.Identificacion}{cliente.Nombre.Substring(0, 2).ToLower()}{cliente.Apellido[0].ToString().ToUpper()}";
            return password == expectedPassword;
        }
    }
}
