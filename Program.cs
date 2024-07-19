using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using MokkilicoresExpressAPI.Models;
using MokkilicoresExpressAPI.Services;
using Microsoft.AspNetCore.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar servicios
        builder.Services.AddControllers();
        // Registro del servicio ITokenService
        builder.Services.AddSingleton<ITokenService, TokenService>();

        // Registro de IMemoryCache
        builder.Services.AddMemoryCache();

        // Configurar proveedores de logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddEventSourceLogger();

        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configurar autenticación con JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Configurar autorización
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
        });

        var app = builder.Build();

        // Configurar el pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        };

        app.UseRouting();

        app.UseExceptionHandler(a => a.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature.Error;

            //_logger.LogError(exception, "Ocurrió un error no manejado");

            await context.Response.WriteAsJsonAsync(new { error = "An error occurred while processing your request." });
        }));


        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        InitializeCache(app.Services);

        app.Run();

        void InitializeCache(IServiceProvider services)
        {
            var cache = services.GetRequiredService<IMemoryCache>();
            const string ClienteCacheKey = "Clientes";
            const string InventarioCacheKey = "Inventarios";
            const string DireccionCacheKey = "Direcciones";

            if (!cache.TryGetValue(ClienteCacheKey, out List<Cliente> _))
            {
                List<Cliente> initialClientes = new List<Cliente>
        {
            new Cliente { Id=1, Identificacion = "801460952", Nombre = "Ignacio", Apellido = "Fernandez", Provincia = "Heredia", Canton = "Flores", Distrito = "San Joaquin" },
            new Cliente { Id=0, Identificacion = "admin", Nombre = "Admin", Apellido = "Admin", Provincia = "_", Canton = "_", Distrito = "_"}
        };
                cache.Set(ClienteCacheKey, initialClientes, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
            }
            if (!cache.TryGetValue(InventarioCacheKey, out List<Inventario> _))
            {
                List<Inventario> initialInventarios = new List<Inventario>
        {
            new Inventario { Id = 1, CantidadEnExistencia = 100, BodegaId = 1, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Champagne" },
            new Inventario { Id = 2, CantidadEnExistencia = 500, BodegaId = 2, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Vino Tinto" },
            new Inventario { Id = 3, CantidadEnExistencia = 150, BodegaId = 4, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Whisky" },
            new Inventario { Id = 2, CantidadEnExistencia = 520, BodegaId = 3, FechaIngreso = DateTime.Now, FechaVencimiento = DateTime.Now.AddYears(1), TipoLicor = "Cerveza" }
        };
                cache.Set(InventarioCacheKey, initialInventarios, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
            }

            if (!cache.TryGetValue(DireccionCacheKey, out List<Direccion> _))
            {
                List<Direccion> initialDirecciones = new List<Direccion>
        {
            new Direccion { Id = 0, ClienteId = 0, Provincia = "Heredia", Canton = "Flores", Distrito = "San Joaquin", PuntoEnWaze = "waze://?ll=9.8998,-83.4444", EsCondominio = false, EsPrincipal = true },
            new Direccion { Id = 1, ClienteId = 1, Provincia = "San Jose", Canton = "San Jose", Distrito = "San Jose", PuntoEnWaze = "waze://?ll=9.9325,-84.0796", EsCondominio = true, EsPrincipal = false }
        };
                cache.Set(DireccionCacheKey, initialDirecciones, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
            }

        }
    }
}