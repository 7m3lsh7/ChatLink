using Microsoft.AspNetCore.Mvc;
using webchat.data;

namespace webchat.Controllers
{
    public class LoginController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        public LoginController(ChatDbcontect chatDbcontect)
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
                return RedirectToAction("Index", "Home");  
            }
            return View();
        }

       [HttpPost]
       [ValidateAntiForgeryToken]
       public IActionResult Check(User user)
       {
          var response = _chatDbcontect.users.FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.PasswordHash);
            if (response == null)
            {
                return NotFound();
            }
            else
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7), 
                    HttpOnly = true
                };
                Response.Cookies.Append("UserId", response.Id.ToString(), cookieOptions);
                Response.Cookies.Append("IsAdmin", response.IsAdmin.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddHours(1)
                });
                return RedirectToAction("Index", "Home"); 
            }

       }
        

        public IActionResult ViewUser()
        {
            var isAdminCookie = Request.Cookies["IsAdmin"];
            if (string.IsNullOrEmpty(isAdminCookie) || isAdminCookie != "True")
            {
                return RedirectToAction("Index", "Login");
            }
            var response = _chatDbcontect.users.ToList();
            return View(response);
        }
       
        public IActionResult Delete(int id)
        {
            var response = _chatDbcontect.users.Find(id);
            _chatDbcontect.users.Remove(response);
            _chatDbcontect.SaveChanges();
            return RedirectToAction("ViewUser", "Login");
        }
                  
    }                      
}
