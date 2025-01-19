using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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

        public IActionResult Cooky()
        {
            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {

            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                }
            }

            return View();
        }
        public IActionResult Chat(int receiverId)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            ViewBag.ReceiverId = receiverId;

            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);
                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);
                ViewBag.SenderId  = int.Parse(userIdCookie);

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
                        .Where(c => c.SenderId == userId  || c.ReceiverId == userId )
                        .OrderByDescending(c => c.Timestamp)
                        .ToList();

                    var uniqueUsers = allMessages
                        .Select(m => m.SenderId == userId  ? m.ReceiverId : m.SenderId)
                        .Distinct()
                        .ToList();

                     var users = _chatDbcontect.users.Where(u => uniqueUsers.Contains(u.Id )).ToList();

                    ViewData["Users"] = users;
                    ViewData["AllMessages"] = allMessages;

                    var messages = _chatDbcontect.chats
                         .AsNoTracking()
                        .Where(c => (c.SenderId == userId && c.ReceiverId == receiverId ) ||
                                    (c.SenderId == receiverId && c.ReceiverId == userId ))
                        .OrderBy(c => c.Timestamp)
                        .ToList();
                     ViewData["Messages"] = messages;

                    var unseenMessages = _chatDbcontect.chats
                        .Where(c => c.ReceiverId == userId && !c.IsRead)
                        .Select(c => new
                        {
                            Id = c.Id,
                            SenderName = ViewData["ReceiverName"],
                            Content = c.Content,
                            Timestamp = c.Timestamp
                        })
                        .ToList();
                    return View(user);
                }
            }
            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var message = _chatDbcontect.chats.FirstOrDefault(c => c.Id == id);
            if (message != null)
            {
                message.IsRead = true;
                _chatDbcontect.SaveChanges();
                return Ok();
            }
            return BadRequest("Message not found");
        }

    }
}
                    