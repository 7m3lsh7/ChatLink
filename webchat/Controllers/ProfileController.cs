using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webchat.data;
using webchat.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;

namespace webchat.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        private readonly IDataProtector _protector;


        public ProfileController(ChatDbcontect chatDbcontect, IDataProtectionProvider provider)
        {
            _chatDbcontect = chatDbcontect;
            _protector = provider.CreateProtector("CookieProtection");
        }

        public IActionResult Index()
        {
            var isAdminCookie = Request.Cookies["IsAdmin"];
            ViewData["IsAdmin"] = isAdminCookie;
            HttpContext.Session.SetString("Profile", "Index");

            var cookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[cookieName];

            if (!string.IsNullOrEmpty(encryptedUserId))
            {
                try
                {
                    var protector = _protector.CreateProtector("UserIdProtector");
                    var decryptedUserId = protector.Unprotect(encryptedUserId);

                    if (int.TryParse(decryptedUserId, out int userId))
                    {
                        var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                        if (user != null)
                        {
                            ViewData["time"] = user.TimeZone;
                            ViewData["nickname"] = user.NickName;
                            ViewData["Photo"] = user.ProfilePicture;

                            return View(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid or missing UserId cookie: {encryptedUserId}");
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
            var cookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[cookieName];
            ViewData["UserID"] = encryptedUserId;

            if (!string.IsNullOrEmpty(encryptedUserId))
            {
                try
                {
                    var protector = _protector.CreateProtector("UserIdProtector");
                    var decryptedUserId = protector.Unprotect(encryptedUserId);

                    if (int.TryParse(decryptedUserId, out int userId))
                    {
                        var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                        if (user != null)
                        {
                            ViewData["nickname"] = user.NickName;
                            ViewData["Photo"] = user.ProfilePicture;
                            ViewData["time"] = user.TimeZone;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid or missing UserId cookie: {encryptedUserId}");
            }

            var users = string.IsNullOrEmpty(username)
               ? new List<User>()
               : _chatDbcontect.users.Where(u => u.Username.Contains(username)).ToList();

            return View(users);
        }

        [HttpPost]
        public IActionResult DeleteAccount([FromBody] DeleteAccountModel model)
        {
            var cookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[cookieName];
            if (string.IsNullOrEmpty(encryptedUserId))
                return Unauthorized();

            try
            {
                var protector = _protector.CreateProtector("UserIdProtector");
                var decryptedUserId = protector.Unprotect(encryptedUserId);

                if (int.TryParse(decryptedUserId, out int userId))
                {
                    var user = _chatDbcontect.users.Find(userId);
                    if (user == null) return NotFound();

                    if (user.PasswordHash != model.Password)
                        return BadRequest(new { success = false, message = "Incorrect password." });

                    _chatDbcontect.users.Remove(user);
                    _chatDbcontect.SaveChanges();

                    Response.Cookies.Delete(cookieName);

                    HttpContext.Session.SetString("Profile", "Deleted");

                    return Json(new { success = true, redirectUrl = Url.Action("Deleted", "Profile") });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                return Unauthorized();
            }

            return Unauthorized();
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

            var cookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[cookieName];
            if (string.IsNullOrEmpty(encryptedUserId))
            {
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var protector = _protector.CreateProtector("UserIdProtector");
                var decryptedUserId = protector.Unprotect(encryptedUserId);

                if (int.TryParse(decryptedUserId, out int userId))
                {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                TempData["ErrorMessage"] = "Unauthorized access.";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Unauthorized access.";
            return RedirectToAction("Index", "Home");
        }

    }
}
