using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using webchat.data;
using webchat.Models;

namespace webchat.Controllers
{
    public class HomeController : Controller
    {
        // Declare the database context to interact with the database
        private readonly ChatDbcontect _chatDbcontect;

        // Constructor to initialize the database context
        public HomeController(ChatDbcontect chatDbcontect)
        {
            _chatDbcontect = chatDbcontect;
        }

        // Action method to handle checking if the user is authenticated via cookie
        public IActionResult Cooky()
        {
            // Get the UserId from cookies
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                // Look up the user in the database by UserId
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    // If the user exists, redirect to the Index page
                    return RedirectToAction("Index");
                }
            }

            // If no user found or not authenticated, redirect to Index
            return RedirectToAction("Index");
        }

        // Action method to display the home page
        public IActionResult Index()
        {
            // Get the UserId from cookies to identify the logged-in user
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                // Retrieve the user from the database using the UserId
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    // Set user-related information for the view (time zone, nickname, profile photo)
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;
                }
            }

            // Return the home view (Index)
            return View();
        }

        // Action method to handle the chat page between users
        public IActionResult Chat(int receiverId)
        {
            // Hide navbar and footer for the chat view
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            // Pass the receiver's Id to the view
            ViewBag.ReceiverId = receiverId;

            // Get the UserId from cookies to identify the logged-in user
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                // Retrieve the sender (logged-in user) from the database
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);
                // Store sender ID for further use in the view
                ViewBag.SenderId = int.Parse(userIdCookie);

                // Retrieve the receiver user from the database
                var receiverUser = _chatDbcontect.users.FirstOrDefault(u => u.Id == receiverId);
                if (receiverUser != null)
                {
                    // Set receiver-related information for the view (nickname, profile photo)
                    ViewData["ReceiverName"] = receiverUser.NickName;
                    ViewData["ReceiverPhoto"] = receiverUser.ProfilePicture;
                }

                if (user != null)
                {
                    // Set user-related information for the view (time zone, nickname, profile photo)
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                    // Retrieve all chat messages for the user, sorted by timestamp
                    var allMessages = _chatDbcontect.chats
                        .AsNoTracking()
                        .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();

                    // Find all unique users involved in chats with the logged-in user
                    var uniqueUsers = allMessages
                        .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                        .Distinct()
                        .ToList();

                    // Retrieve the list of users involved in the chats
                    var users = _chatDbcontect.users.Where(u => uniqueUsers.Contains(u.Id)).ToList();
                    ViewData["Users"] = users;

                    // Pass all messages to the view
                    ViewData["AllMessages"] = allMessages;

                    // Retrieve the specific messages between the sender and receiver, sorted by timestamp
                    var messages = _chatDbcontect.chats
                        .AsNoTracking()
                        .Where(c => (c.SenderId == userId && c.ReceiverId == receiverId) ||
                                    (c.SenderId == receiverId && c.ReceiverId == userId))
                        .OrderBy(c => c.Timestamp)
                        .ToList();
                    ViewData["Messages"] = messages;

                    // Return the Chat view for the logged-in user and the selected receiver
                    return View(user);
                }
            }

            // If no valid user found or not authenticated, redirect to the home page
            return RedirectToAction("Index", "Home");
        }
    }
}
