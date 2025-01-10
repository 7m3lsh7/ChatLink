using Microsoft.AspNetCore.Mvc;

namespace webchat.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Logout()
        {
            Response.Cookies.Delete("UserId");
            return RedirectToAction("Index", "Home");
        }
    }
}
