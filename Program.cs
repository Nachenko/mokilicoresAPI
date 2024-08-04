using Microsoft.EntityFrameworkCore;
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

        // Configurar EF Core con SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        }

        app.UseRouting();

        app.UseExceptionHandler(a => a.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature.Error;

            //_logger.LogError(exception, "Ocurrió un error no manejado");

            await context.Response.WriteAsJsonAsync(new { error = "An error occurred while processing your request." });
        }));

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        InitializeCache(app.Services);

        app.Run();

        void InitializeCache(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var cache = scopedServices.GetRequiredService<IMemoryCache>();
                var context = scopedServices.GetRequiredService<ApplicationDbContext>();

                const string ClienteCacheKey = "Clientes";
                const string InventarioCacheKey = "Inventarios";
                const string DireccionCacheKey = "Direcciones";

                if (!cache.TryGetValue(ClienteCacheKey, out List<Cliente> clientes))
                {
                    clientes = context.Clientes.ToList();
                    cache.Set(ClienteCacheKey, clientes, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
                }

                if (!cache.TryGetValue(InventarioCacheKey, out List<Inventario> inventarios))
                {
                    inventarios = context.Inventarios.ToList();
                    cache.Set(InventarioCacheKey, inventarios, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
                }

                if (!cache.TryGetValue(DireccionCacheKey, out List<Direccion> direcciones))
                {
                    direcciones = context.Direcciones.ToList();
                    cache.Set(DireccionCacheKey, direcciones, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60)));
                }
            }
        }
    }
}
