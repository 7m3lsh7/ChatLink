using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webchat.data;
using webchat.Models;

namespace webchat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;

        public HomeController(ChatDbcontect chatDbcontect)
        {
            _chatDbcontect = chatDbcontect;
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
        public IActionResult Chat(int receiverId)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            ViewBag.ReceiverId = receiverId;

            var userIdCookie = Request.Cookies["UserId"];
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
    }
}
