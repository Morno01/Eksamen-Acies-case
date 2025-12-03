using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    [Authorize]
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

        public IActionResult Serier()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}
