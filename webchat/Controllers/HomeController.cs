using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webchat.data;
using webchat.Models;

namespace webchat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;

        public HomeController(ChatDbcontect chatDbcontect , IWebHostEnvironment env, IDataProtectionProvider provider)
        {
            _chatDbcontect = chatDbcontect;
            _env = env;
            _protector = provider.CreateProtector("CookieProtection");
        }

        // Action method to handle checking if the user is authenticated via cookie
        public IActionResult Cooky()
        {
            var cookieName = "p9q8r7s6_t34w2x1";
            
            var encryptedUserId = Request.Cookies[cookieName];

            if (!string.IsNullOrEmpty(encryptedUserId))
            {
                try
                {
                    // فك تشفير الكوكيز
                    var protector = _protector.CreateProtector("UserIdProtector");
                    var decryptedUserId = protector.Unprotect(encryptedUserId);

                    if (int.TryParse(decryptedUserId, out int userId))
                    {
                        var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                        if (user != null)
                        {
                            return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }

        // Action method to display the home page
        public IActionResult Index()
        {
            var userIdCookieName = "p9q8r7s6_t34w2x1";
            var isAdminCookieName = "m3n2b1_a0q9w8";

            var encryptedUserId = Request.Cookies[userIdCookieName];
            var encryptedIsAdmin = Request.Cookies[isAdminCookieName];

            string decryptedUserId = null;
            string decryptedIsAdmin = null;

            try
            {
                var protector = _protector.CreateProtector("UserIdProtector");
                if (!string.IsNullOrEmpty(encryptedUserId))
                {
                    decryptedUserId = protector.Unprotect(encryptedUserId);
                    ViewData["UserID"] = decryptedUserId;
                }

                if (!string.IsNullOrEmpty(encryptedIsAdmin))
                {
                    decryptedIsAdmin = protector.Unprotect(encryptedIsAdmin);
                    ViewData["IsAdmin"] = decryptedIsAdmin;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting cookies: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(decryptedUserId) && int.TryParse(decryptedUserId, out int userId))
            {
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;
                }
            }
            else
            {
                Console.WriteLine($"Invalid or missing UserId cookie: {decryptedUserId}");
            }

            return View();
        }


        public IActionResult Chat(int receiverId)
        {
            ViewData["HideFooter"] = true;
            ViewData["HideClock"] = true;
            ViewBag.ReceiverId = receiverId;

            var userIdCookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[userIdCookieName];

            string decryptedUserId = null;

            try
            {
                var protector = _protector.CreateProtector("UserIdProtector");
                if (!string.IsNullOrEmpty(encryptedUserId))
                {
                    decryptedUserId = protector.Unprotect(encryptedUserId);
                    ViewData["UserID"] = decryptedUserId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(decryptedUserId) && int.TryParse(decryptedUserId, out int userId))
            {
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);
                ViewBag.SenderId = userId;

                var receiverUser = _chatDbcontect.users.FirstOrDefault(u => u.Id == receiverId);
                if (receiverUser != null)
                {
                    ViewData["ReceiverName"] = receiverUser.NickName;
                    ViewData["ReceiverPhoto"] = receiverUser.ProfilePicture;
                }

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                    var allMessages = _chatDbcontect.chats
                        .AsNoTracking()
                        .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();
                    ViewData["AllMessages"] = allMessages;

                    var uniqueUsers = allMessages
                        .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                        .Distinct()
                        .ToList();

                    var users = _chatDbcontect.users.Where(u => uniqueUsers.Contains(u.Id)).ToList();
                    ViewData["Users"] = users;

                    var messages = _chatDbcontect.chats
                        .AsNoTracking()
                        .Where(c => (c.SenderId == userId && c.ReceiverId == receiverId) ||
                                    (c.SenderId == receiverId && c.ReceiverId == userId))
                        .OrderBy(c => c.Timestamp)
                        .ToList();

                    foreach (var chat in messages)
                    {
                        if (chat.MessageType == "file" && !chat.Content.StartsWith("http"))
                        {
                            chat.Content = $"{Request.Scheme}://{Request.Host}{chat.Content}";
                        }
                    }
                    ViewData["Messages"] = messages;

                    return View(user);
                }
            }
            else
            {
                Console.WriteLine($"Invalid or missing UserId cookie: {decryptedUserId}");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string fileUrl = "/uploads/" + uniqueFileName;
            return Ok(fileUrl);
        }
    }

}
