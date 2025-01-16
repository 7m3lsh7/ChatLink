using Microsoft.AspNetCore.Mvc;
using webchat.data;

namespace webchat.Controllers
{
    public class HomeController : Controller
    {

        private readonly ChatDbcontect _chatDbcontect;
        public HomeController(ChatDbcontect chatDbcontect)                           
        {
            _chatDbcontect = chatDbcontect;
        }

        public IActionResult Cooky()
        {
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {

            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                }
            }

            return View();
        }    
        
        public IActionResult Chat()
        {

            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                }
            }
            return View();

        }
    }                     
}
                    