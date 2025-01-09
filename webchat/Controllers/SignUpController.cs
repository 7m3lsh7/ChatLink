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

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // التأكد من أن البريد الإلكتروني غير مسجل مسبقًا
            if (_chatDbcontect.users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل بالفعل.");
                return View("Index");
            }

            // توليد رمز التحقق وتخزين تاريخ التسجيل
            user.VerificationToken = Guid.NewGuid().ToString();
            user.CrearetedAt = DateTime.Now;

            // إضافة المستخدم إلى قاعدة البيانات
            _chatDbcontect.users.Add(user);
            await _chatDbcontect.SaveChangesAsync();

            // إرسال البريد الإلكتروني للتحقق
            SendVerificationEmail(user.Email, user.VerificationToken);

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }                      
                           
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string Email, string token)
        {
            var user = _chatDbcontect.users.FirstOrDefault(u => u.Email == Email && u.VerificationToken == token);
            if (user == null)
                return BadRequest("رابط التحقق غير صالح أو منتهي الصلاحية.");

            user.IsEmailVerified = true;
            user.VerificationToken = null; // إزالة رمز التحقق بعد التحقق
            await _chatDbcontect.SaveChangesAsync();

            return RedirectToAction("Verified");
        }

        public IActionResult Verified()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        private void SendVerificationEmail(string email, string token)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Student Portal", "your_email@example.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Verify Your Email";

            string verificationLink = Url.Action("VerifyEmail", "SignUp", new { email, token }, Request.Scheme);
            message.Body = new TextPart("plain")
            {
                Text = $"Please click the following link to verify your email: {verificationLink}"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false); // تأكد من استخدام البروتوكول الصحيح
                client.Authenticate("felopater.remon2020@gmail.com", "575OscarO575"); // استخدم كلمة مرور التطبيق هنا
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
                                