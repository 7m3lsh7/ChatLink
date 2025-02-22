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
using BCrypt.Net;
using Microsoft.AspNetCore.DataProtection;

namespace webchat.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ChatDbcontect _chatDbContext;
        private readonly IDataProtector _protector;


        public SignUpController(ChatDbcontect chatDbContext, IDataProtectionProvider provider)
        {
            _chatDbContext = chatDbContext;
            _protector = provider.CreateProtector("CookieProtection");
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
                string htmlBody = $@"
                    <html>
                    <body>
                        <h1>Hello!</h1>
                        <p>Thank you for registering on our website. Please use the following code to verify your email:</p>
                        <h2>{body}</h2>
                        <p>If you did not request this code, please ignore this message.</p>
                        <p>Best regards,<br/>The Support Team</p>
                    </body>
                    </html>";

                MailMessage mail = new MailMessage("ChatLink.eg@gmail.com", toEmail);
                mail.Subject = subject;
                mail.Body = htmlBody;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new System.Net.NetworkCredential("ChatLink.eg@gmail.com", "ffhm glyt bcup ddke");
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        private void SendWelcomeEmail(string toEmail, string username)
        {
            try
            {
                string subject = "Welcome to Our Website!";
                string body = $@"
                    <html>
                    <body>
                        <h1>Hello {username}!</h1>
                        <p>Thank you for registering on our website. We are excited to have you on board.</p>
                        <p>You can now enjoy all the features of our platform.</p>
                        <p>If you have any questions, feel free to contact us.</p>
                        <p>Best regards,<br/>The Support Team</p>
                    </body>
                    </html>";

                MailMessage mail = new MailMessage("ChatLink.eg@gmail.com", toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new System.Net.NetworkCredential("ChatLink.eg@gmail.com", "ffhm glyt bcup ddke");
                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
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
                ModelState.AddModelError("Email", "This email is already registered.");
                return View("Index");
            }

            // Hash the password using BCrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            var verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10);
            user.IsVerified = false;

            HttpContext.Session.Set("UserStep1", user);
            HttpContext.Session.SetString("SignUpStep", "Addphoto");

            var emailBody = $"Your verification code is: {verificationCode}";
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
            user.IsAdmin = "false";
            user.ResetPasswordToken = "";
            user.ResetPasswordExpiry = null;
            user.IsPasswordChanged = true;
            user.LastPasswordChangeDate = DateTime.Now;

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
                ModelState.AddModelError("", "reCAPTCHA verification failed. Please try again.");
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
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true
                };

                var protector = _protector.CreateProtector("UserIdProtector");

                var encryptedUserId = protector.Protect(userStep2.Id.ToString());
                var encryptedUsername = protector.Protect(userStep2.Username);

                Response.Cookies.Append("p9q8r7s6_t34w2x1", encryptedUserId, cookieOptions);
                Response.Cookies.Append("UsernameCookie", encryptedUsername, cookieOptions);

                SendWelcomeEmail(userStep2.Email, userStep2.Username);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("Code", "The code is incorrect or has expired.");
            return View();
        }

    }
}
