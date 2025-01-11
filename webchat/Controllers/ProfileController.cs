using Microsoft.AspNetCore.Mvc;
using webchat.data;

namespace webchat.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;

        public ProfileController(ChatDbcontect chatDbcontect)
        {
            _chatDbcontect = chatDbcontect;
        }

        public IActionResult Index()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;

            var userIdCookie = Request.Cookies["UserId"];

            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);

                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    return View(user);
                }
            }

            return RedirectToAction("Index", "Login");
        }


        public IActionResult Edit(int id) 
        {

            var response = _chatDbcontect.users.FirstOrDefault(u => u.Id == id);

            if (response == null)
            {
                return NotFound();
            }

            return View(response);
        }

        [HttpPost]
        public IActionResult Edit(User user, int id) 
        {
            var response = _chatDbcontect.users.Find(id);

            if (response == null)
            {
                return NotFound();
            }

            else
            { 
                response.Email = user.Email;
                response.Username = user.Username;
                response.NickName = user.NickName;
                response.Country = user.Country;
                response.Language = user.Language;
                response.PasswordHash = user.PasswordHash;
                response.TimeZone = user.TimeZone;
                response.Gender = user.Gender;
                _chatDbcontect.SaveChanges();
                return RedirectToAction("Index","Profile");
            }
        }

        public IActionResult AddEmail(int id)
        {
            var response = _chatDbcontect.users.FirstOrDefault(u => u.Id == id);

            if (response == null)
            {
                return NotFound();
            }

            return View(response);
        }

        [HttpPost]
        public IActionResult AddEmail(User user, int id)
        {
            var response = _chatDbcontect.users.Find(id);

            if (response == null)
            {
                return NotFound();
            }
            else
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    
                    if (!string.IsNullOrEmpty(response.Email))
                    {
                        response.Email += "\n" + user.Email;
                    }
                    else
                    {
                        response.Email = user.Email; 
                    }
                }

                _chatDbcontect.SaveChanges();
                return RedirectToAction("Index", "Profile"); 
            }
        }

    }
}
