using Microsoft.EntityFrameworkCore;

namespace MokkilicoresExpressAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
    }
}
