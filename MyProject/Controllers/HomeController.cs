using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Paller()
        {
            return View();
        }

        public IActionResult Elementer()
        {
            return View();
        }

        public IActionResult Optimering()
        {
            return View();
        }
    }
}
