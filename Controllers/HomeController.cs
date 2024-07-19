using Microsoft.AspNetCore.Mvc;

namespace MokkilicoresExpressAPI.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Content("Bienvenido a Mokkilicores Express API");
        }
    }
}
