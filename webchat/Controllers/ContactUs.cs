using Microsoft.AspNetCore.Mvc;
using webchat.data;
using webchat.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.DataProtection;

namespace webchat.Controllers
{
    public class ContactUs : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        private readonly IDataProtector _protector;


        public ContactUs(ChatDbcontect chatDbcontect, IDataProtectionProvider provider)
        {
            _chatDbcontect = chatDbcontect;
            _protector = provider.CreateProtector("CookieProtection");

        }

        public IActionResult Index()
        {
            var isAdminCookie = Request.Cookies["IsAdmin"];
            ViewData["IsAdmin"] = isAdminCookie;
            HttpContext.Session.SetString("ContactUs", "Index");

            var cookieName = "p9q8r7s6_t34w2x1";

            var encryptedUserId = Request.Cookies[cookieName];
            ViewData["UserID"] = encryptedUserId;

            if (!string.IsNullOrEmpty(encryptedUserId))
            {
                try
                {
                    var protector = _protector.CreateProtector("UserIdProtector");
                    var decryptedUserId = protector.Unprotect(encryptedUserId);

                    if (int.TryParse(decryptedUserId, out int userId))
                    {
                        var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                        if (user != null)
                        {
                            ViewData["time"] = user.TimeZone;
                            ViewData["nickname"] = user.NickName;
                            ViewData["Photo"] = user.ProfilePicture;

                            return View(new ContactModel());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                }
            }

            return View(new ContactModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ContactModel model)
        {
            if (!ModelState.IsValid)
            {
                var cookieName = "p9q8r7s6_t34w2x1";

                var encryptedUserId = Request.Cookies[cookieName];
                ViewData["UserID"] = encryptedUserId;

                if (!string.IsNullOrEmpty(encryptedUserId))
                {
                    try
                    {
                        var protector = _protector.CreateProtector("UserIdProtector");
                        var decryptedUserId = protector.Unprotect(encryptedUserId);

                        if (int.TryParse(decryptedUserId, out int userId))
                        {
                            var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                            if (user != null)
                            {
                                ViewData["time"] = user.TimeZone;
                                ViewData["nickname"] = user.NickName;
                                ViewData["Photo"] = user.ProfilePicture;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting UserId cookie: {ex.Message}");
                    }
                }

                return View("Index", model);
            }

            SendEmailToCompany(model);

            SaveContactFormData(model);
            HttpContext.Session.SetString("ContactUs", "ThankYou");

            return RedirectToAction("ThankYou");
        }


        public IActionResult ThankYou()
        {
            if (HttpContext.Session.GetString("ContactUs") != "ThankYou")
            {
                return RedirectToAction("Index");
            }

            var userIdCookie = Request.Cookies["UserId"];
            ViewData["UserID"] = userIdCookie;

            if (userIdCookie != null)
            {
                var userId = int.Parse(userIdCookie);

                var user = _chatDbcontect.users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewData["time"] = user.TimeZone;
                    ViewData["nickname"] = user.NickName;
                    ViewData["Photo"] = user.ProfilePicture;

                    return View(new ContactModel());
                }
            }

            return RedirectToAction("Index", "Login");
        }


        private void SendEmailToCompany(ContactModel model)
        {
            try
            {
                var fromAddress = new MailAddress("ChatLink.eg@gmail.com", "ChatLink Support"); 
                var toAddress = new MailAddress("ChatLink.eg@gmail.com", "ChatLink Support"); 
                const string fromPassword = "ffhm glyt bcup ddke"; 

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("ChatLink.eg@gmail.com", fromPassword) 
                };

                string emailSubject = $"New Contact Form Submission - {model.Name}";

                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                        <div style='background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                            <h2 style='color: #333;'>New Contact Form Submission</h2>
                            <p style='color: #555;'>You have received a new message from the contact form on your website. Below are the details:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Name:</strong></td>
                                    <td style='padding: 10px; border: 1px solid #ddd;'>{model.Name}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Email:</strong></td>
                                    <td style='padding: 10px; border: 1px solid #ddd;'>{model.Email}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Mobile:</strong></td>
                                    <td style='padding: 10px; border: 1px solid #ddd;'>{model.Mobile}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border: 1px solid #ddd; background-color: #f9f9f9;'><strong>Message:</strong></td>
                                    <td style='padding: 10px; border: 1px solid #ddd;'>{model.AdditionalInfo}</td>
                                </tr>
                            </table>

                            <p style='color: #555; margin-top: 20px;'>This message was sent from the contact form on your website. Please respond to the sender if necessary.</p>
                        </div>
                    </body>
                    </html>";

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = emailSubject,
                    Body = emailBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        private void SaveContactFormData(ContactModel model)
        {
            var contactFormData = new ContactModel
            {
                Name = model.Name,
                Mobile = model.Mobile,
                Email = model.Email,
                AdditionalInfo = model.AdditionalInfo,
                SubmissionDate = DateTime.Now
            };

            _chatDbcontect.contacts.Add(contactFormData);
            _chatDbcontect.SaveChanges();
        }
    }
}