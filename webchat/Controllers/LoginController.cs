using Microsoft.AspNetCore.Mvc;

namespace webchat.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
            //ggg
        }
    }
}
