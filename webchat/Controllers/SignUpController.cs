using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webchat.data;
using webchat.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using AspNetCore.ReCaptcha;

namespace webchat.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ChatDbcontect _chatDbContext;

        public SignUpController(ChatDbcontect chatDbContext)
        {
            _chatDbContext = chatDbContext;
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); 
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage("bm9412238@gmail.com", toEmail);
                mail.Subject = subject;
                mail.Body = body;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new System.Net.NetworkCredential("bm9412238@gmail.com", "lvzg aqkt ttvk nngg");
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetString("SignUpStep", "Index");
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        [HttpPost]
        public IActionResult add(User user)
        {
            if (_chatDbContext.users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل بالفعل.");
                return View("Index");
            }

            var verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10);
            user.IsVerified = false;

            HttpContext.Session.Set("UserStep1", user);
            HttpContext.Session.SetString("SignUpStep", "Addphoto");

            var emailBody = $"Your verification code is:( {verificationCode} )";
            SendEmail(user.Email, "Verify your email", emailBody);

            return RedirectToAction("Addphoto");
        }

        public IActionResult Addphoto()
        {
            if (HttpContext.Session.GetString("SignUpStep") != "Addphoto")
            {
                return RedirectToAction("Index");
            }

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
            user.NickName = userStep1.NickName;
            user.Country = userStep1.Country;
            user.Gender = userStep1.Gender;
            user.Language = userStep1.Language;
            user.TimeZone = userStep1.TimeZone;
            user.CrearetedAt = DateTime.Now;
            user.VerificationCode = userStep1.VerificationCode;
            user.VerificationCodeExpiry = userStep1.VerificationCodeExpiry;
            user.IsVerified = false;
            user.IsAdmin = false;

            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads", ProfilePicture.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }
                user.ProfilePicture = $"/uploads/{ProfilePicture.FileName}";
            }

           
            HttpContext.Session.Set("UserStep2", user);
            HttpContext.Session.SetString("SignUpStep", "VerifyEmail");

          
            return RedirectToAction("VerifyEmail");
        }

        public IActionResult VerifyEmail()
        {
            if (HttpContext.Session.GetString("SignUpStep") != "VerifyEmail")
            {
                return RedirectToAction("Index");
            }

            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        [HttpPost]
        [ValidateReCaptcha]
        public IActionResult VerifyEmail(string code)
        {
            var userStep2 = HttpContext.Session.Get<User>("UserStep2");
            if (userStep2 == null)
            {
                return RedirectToAction("Index");
            }

           
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "التحقق من reCAPTCHA فشل. يرجى المحاولة مرة أخرى.");
                return View();
            }

            
            if (userStep2.VerificationCode == code && userStep2.VerificationCodeExpiry > DateTime.Now)
            {
                userStep2.IsVerified = true; 


                _chatDbContext.users.Add(userStep2);
                _chatDbContext.SaveChanges();

                HttpContext.Session.SetInt32("UserId", userStep2.Id); 
                HttpContext.Session.SetString("Username", userStep2.Username); 

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30), 
                    IsEssential = true 
                };

                Response.Cookies.Append("UserId", userStep2.Id.ToString(), cookieOptions);
                Response.Cookies.Append("Username", userStep2.Username, cookieOptions);

                return RedirectToAction("Cooky", "Home");
            }

            ModelState.AddModelError("Code", "الكود غير صحيح أو منتهي الصلاحية.");
            return View();
        }
    }
}