namespace MokkilicoresExpressAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public decimal DineroCompradoTotal { get; set; }
        public decimal DineroCompradoUltimoAno { get; set; }
        public decimal DineroCompradoUltimos6Meses { get; set; }
    }
}
