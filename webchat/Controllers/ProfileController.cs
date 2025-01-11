using Microsoft.AspNetCore.Mvc;

namespace webchat.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
