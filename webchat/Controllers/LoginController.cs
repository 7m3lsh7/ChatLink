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
                return RedirectToAction("ViewUser", "Login");
            }
        }
        

        public IActionResult ViewUser()
        {
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
