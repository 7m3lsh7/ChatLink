using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webchat.data;
using webchat.Models;
using Microsoft.AspNetCore.Http;

namespace webchat.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;

        public SignUpController(ChatDbcontect chatDbcontect)
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
        public IActionResult add(User user)
        {
            if (_chatDbcontect.users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل بالفعل.");
                return View("Index");
            }

            HttpContext.Session.Set("UserStep1", user);
            return RedirectToAction("Addphoto");
        }
                                                  
        public IActionResult Addphoto()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Finish(User user, IFormFile ProfilePicture)
        {
            var userStep1 = HttpContext.Session.Get<User>("UserStep1");
            if (userStep1 == null)
            {
                return RedirectToAction("Index");
            }

            user.Username = userStep1.Username;
            user.Email = userStep1.Email;
            user.PasswordHash = userStep1.PasswordHash;
            user.CrearetedAt = DateTime.Now;

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads", ProfilePicture.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }
                user.ProfilePicture = $"/uploads/{ProfilePicture.FileName}";
            }

            _chatDbcontect.users.Add(user);
            await _chatDbcontect.SaveChangesAsync();


            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(30) 
            };

            Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);

            return RedirectToAction("Cooky", "Home");
        }
    }
}
                           