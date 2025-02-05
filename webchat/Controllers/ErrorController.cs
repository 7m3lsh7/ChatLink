using Microsoft.AspNetCore.Mvc;

namespace webchat.Controllers
{
    public class ErrorController : Controller
    {

        [Route("Error")]
        public IActionResult General()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            Response.StatusCode = 500;
            return View();
        }

        [Route("Error/404")]
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public IActionResult NotFound()
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/500")]
        public IActionResult ServerError()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            Response.StatusCode = 500;
            return View();
        }

        public IActionResult Index()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return RedirectToAction("General");
        }
    }
}
