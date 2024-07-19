using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Services;
using System.Collections.Generic;
using System.Linq;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ITokenService _tokenService;
        private const string ClienteCacheKey = "Clientes";

        public AccountController(IMemoryCache cache, ITokenService tokenService)
        {
            _cache = cache;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest.Identificacion == "admin" && loginRequest.Password == "admin")
            {
                return Ok(new LoginResponse { Token = _tokenService.GenerateJwtToken("admin", "Admin"), Identificacion = "admin", Role = "Admin" });
            }

            var clientes = _cache.Get<List<Cliente>>(ClienteCacheKey);
            var cliente = clientes?.FirstOrDefault(c => c.Identificacion == loginRequest.Identificacion);
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
