using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webchat.data;
using webchat.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
            var isAdminCookie = Request.Cookies["IsAdmin"];
            ViewData["IsAdmin"] = isAdminCookie;
            HttpContext.Session.SetString("Profile", "Index");

            var userIdCookie = Request.Cookies["UserId"];
            ViewData["UserID"] = userIdCookie;

            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                    return View(user);
                }
            }

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public IActionResult Edit(User user, int id, IFormFile ProfilePicture)
        {
            var response = _chatDbcontect.users.Find(id);
            if (response == null)
            {
                return NotFound();
            }

            response.Username = user.Username;
            response.NickName = user.NickName;
            response.TimeZone = user.TimeZone;

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var fileName = Path.GetFileName(ProfilePicture.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfilePicture.CopyTo(stream);
                }

                response.ProfilePicture = "/uploads/" + fileName;
            }

            _chatDbcontect.SaveChanges();
            return Json(new { success = true, message = "Profile updated successfully" });
        }

        public IActionResult AddEmail(int id)
        {
            var user = _chatDbcontect.users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
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
                user.Email = string.IsNullOrEmpty(user.Email) ? email : user.Email + "\n" + email;
                _chatDbcontect.SaveChanges();
            }

            return RedirectToAction("Index", "Profile");
        }

        [HttpGet]
        public IActionResult Search(string username)
        {
            var userIdCookie = Request.Cookies["UserId"];
            ViewData["UserID"] = userIdCookie;

            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;
                    ViewData["time"] = user.TimeZone;
                }
            }

            var users = string.IsNullOrEmpty(username)
               ? new List<User>()
               : _chatDbcontect.users.Where(u => u.Username.Contains(username)).ToList();

            return View(users);
        }

        [HttpPost]
        public IActionResult DeleteAccount([FromBody] DeleteAccountModel model)
        {
            
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie == null) return Unauthorized();

            var userId = int.Parse(userIdCookie);
            var user = _chatDbcontect.users.Find(userId);
            if (user == null) return NotFound();

            if (user.PasswordHash != model.Password)
                return BadRequest(new { success = false, message = "Incorrect password." });

            _chatDbcontect.users.Remove(user);
            _chatDbcontect.SaveChanges();

            Response.Cookies.Delete("UserId");

            HttpContext.Session.SetString("Profile", "Deleted");

            return Json(new { success = true, redirectUrl = Url.Action("Deleted", "Profile") });
        }

        public IActionResult Deleted()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;

            if (HttpContext.Session.GetString("Profile") != "Deleted")
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;

            var userId = int.Parse(Request.Cookies["UserId"]);
            var user = await _chatDbcontect.users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            bool isOldPasswordValid = false;

            if (user.PasswordHash.StartsWith("$2a$") || user.PasswordHash.StartsWith("$2b$") || user.PasswordHash.StartsWith("$2y$"))
            {
                isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            }
            else
            {
                isOldPasswordValid = oldPassword == user.PasswordHash;
            }

            if (!isOldPasswordValid)
            {
                ViewBag.Message = "Old password is incorrect.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "New password and confirmation do not match.";
                return View();
            }

            if (!Regex.IsMatch(newPassword, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{9,}$"))
            {
                ViewBag.Message = "Password must include both letters and numbers and be at least 9 characters long.";
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.IsPasswordChanged = true;
            user.LastPasswordChangeDate = DateTime.Now; 

            _chatDbcontect.users.Update(user);
            await _chatDbcontect.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}
