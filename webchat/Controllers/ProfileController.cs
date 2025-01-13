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
                 response.Username = user.Username;
                response.NickName = user.NickName;
              
                _chatDbcontect.SaveChanges();
                return Json(new { success = true, message = "Profile updated successfully" });
            }
        }

         public IActionResult AddEmail(int id)
        {
            var user = _chatDbcontect.users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult SaveEmail(int id, string email)
        {
            var user = _chatDbcontect.users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(email))
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    user.Email += "\n" + email;
                }
                else
                {
                    user.Email = email;
                }

                _chatDbcontect.SaveChanges();
            }

            return RedirectToAction("Index", "Profile");
        }
    

    }
}
