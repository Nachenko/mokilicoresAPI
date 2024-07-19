namespace MokkilicoresExpressAPI.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int InventarioId { get; set; }
        public int DireccionId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoSinIVA { get; set; }
        public decimal CostoTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        
    }
}
