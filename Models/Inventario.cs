namespace MokkilicoresExpressAPI.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        public int CantidadEnExistencia { get; set; }
        public int BodegaId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string TipoLicor { get; set; } = string.Empty;
    }
}
