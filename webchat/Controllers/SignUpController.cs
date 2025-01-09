using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webchat.data;
using webchat.Models;
using MimeKit;
using MailKit.Net.Smtp;

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

    }
}
