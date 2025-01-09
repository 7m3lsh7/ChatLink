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

        public IActionResult Index()
        {
            return View();
        }                        
    }                     
}
                    