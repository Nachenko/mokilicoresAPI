namespace MokkilicoresExpressAPI.Models
{
    public class LoginRequest
    {
        public string Identificacion { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Identificacion { get; set; }
        public string Role { get; set; }
    }
}
