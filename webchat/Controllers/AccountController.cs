using Microsoft.AspNetCore.Mvc;

namespace webchat.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Logout()
        {
            var cookieName = "p9q8r7s6_t34w2x1";

            Response.Cookies.Delete(cookieName);

            Response.Cookies.Append(cookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1)
            });

            // إعادة توجيه للصفحة الرئيسية
            return RedirectToAction("Index", "Home");
        }
    }
}
