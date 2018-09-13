using Microsoft.AspNetCore.Mvc;

namespace MacAuth.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Login");
        }
    }
}