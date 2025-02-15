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

        public HomeController(ChatDbcontect chatDbcontect , IWebHostEnvironment env)
        {
            _chatDbcontect = chatDbcontect;
            _env = env;
        }

        // Action method to handle checking if the user is authenticated via cookie
        public IActionResult Cooky()
        {
            var userIdCookie = Request.Cookies["UserId"];
            if (!string.IsNullOrEmpty(userIdCookie) && int.TryParse(userIdCookie, out int userId))
            {
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Log the error for debugging purposes
                Console.WriteLine($"Invalid or missing UserId cookie: {userIdCookie}");
            }

            return RedirectToAction("Index");
        }

        // Action method to display the home page
        public IActionResult Index()
        {
            var userIdCookie = Request.Cookies["UserId"];
            ViewData["UserID"] = userIdCookie;
            var isAdminCookie = Request.Cookies["IsAdmin"];
            ViewData["IsAdmin"] = isAdminCookie;
            if (!string.IsNullOrEmpty(userIdCookie) && int.TryParse(userIdCookie, out int userId))
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
                Console.WriteLine($"Invalid or missing UserId cookie: {userIdCookie}");
            }

            return View();
        }

        // Action method to handle the chat page between users
        public IActionResult Chat( int receiverId)
        {
            ViewData["HideFooter"] = true;
            ViewData["HideClock"] = true;
            ViewBag.ReceiverId = receiverId;
                 
             var userIdCookie = Request.Cookies["UserId"];
            ViewData["UserID"] = userIdCookie;

            if (!string.IsNullOrEmpty(userIdCookie) && int.TryParse(userIdCookie, out int userId))
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
                    ViewData["AllMessages"] =allMessages  ;
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
                Console.WriteLine($"Invalid or missing UserId cookie: {userIdCookie}");
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
