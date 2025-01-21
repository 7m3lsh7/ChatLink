using Microsoft.AspNetCore.Mvc;
using webchat.data;
using webchat.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace webchat.Controllers
{
    public class LoginController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        private readonly IConfiguration _configuration;

        public LoginController(ChatDbcontect chatDbcontect, IConfiguration configuration)
        {
            _chatDbcontect = chatDbcontect;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;

            var userIdCookie = Request.Cookies["UserId"];
            if (userIdCookie != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Check(User user)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            var dbUser = _chatDbcontect.users.FirstOrDefault(u => u.Email == user.Email);

            if (dbUser == null || user.PasswordHash != dbUser.PasswordHash)
            {
                ViewBag.Message = "Incorrect email or password.";
                return View("Index");
            }

            if (!Regex.IsMatch(user.PasswordHash, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{9,}$"))
            {
                ViewBag.Message = "Password must include both letters and numbers and be at least 9 characters long.";
                return View("Index");
            }
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true
            };
            Response.Cookies.Append("UserId", dbUser.Id.ToString(), cookieOptions);
            Response.Cookies.Append("IsAdmin", dbUser.IsAdmin.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddHours(1)
            });

            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _chatDbcontect.users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Email not registered.";
                return View();
            }

           
            var token = Guid.NewGuid().ToString();
            user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.Now.AddHours(1); 
            await _chatDbcontect.SaveChangesAsync();

           
            var resetLink = Url.Action("ResetPassword", "Login", new { token, email = user.Email }, Request.Scheme);
            await SendEmailAsync(user.Email, "Reset password", $"Please click on the following link to reset your password: {resetLink}");

            
            return RedirectToAction("CheckEmailConfirmation");
        }

       
        public IActionResult CheckEmailConfirmation()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

        public IActionResult ResetPassword(string token, string email)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            var user = _chatDbcontect.users.FirstOrDefault(u => u.Email == email && u.ResetPasswordToken == token && u.ResetPasswordExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Message = "الرابط غير صالح أو انتهت صلاحيته.";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string email, string newPassword, string confirmPassword)
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "كلمة المرور غير متطابقة.";
                return View();
            }

            var user = _chatDbcontect.users.FirstOrDefault(u => u.Email == email && u.ResetPasswordToken == token && u.ResetPasswordExpiry > DateTime.Now);
            if (user == null)
            {
                ViewBag.Message = "الرابط غير صالح أو انتهت صلاحيته.";
                return RedirectToAction("ForgotPassword");
            }

            
            user.PasswordHash = newPassword;
            user.ResetPasswordExpiry = null;
            _chatDbcontect.SaveChanges();

            ViewBag.Message = "تم إعادة تعيين كلمة المرور بنجاح.";
            return RedirectToAction("Index");
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("WepChat Reset Password", "wepchat9412238@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync("wepchat9412238@gmail.com", "hvst kokk dyws nksv");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sending email failed: " + ex.Message);
            }
        }

        public IActionResult ViewUser()
        {
            var isAdminCookie = Request.Cookies["IsAdmin"];
            if (string.IsNullOrEmpty(isAdminCookie) || isAdminCookie != "True")
            {
                return RedirectToAction("Index", "Login");
            }
            var response = _chatDbcontect.users.ToList();
            return View(response);
        }

        public IActionResult Delete(int id)
        {
            var response = _chatDbcontect.users.Find(id);
            _chatDbcontect.users.Remove(response);
            _chatDbcontect.SaveChanges();
            return RedirectToAction("ViewUser", "Login");
        }
    }
}