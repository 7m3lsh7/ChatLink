using Azure;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using webchat.data;

public class SignUpController : Controller
{
    private readonly ChatDbcontect _chatDbcontect;

    public SignUpController(ChatDbcontect chatDbcontect)
    {
        _chatDbcontect = chatDbcontect;
    }

    // دالة لإنشاء كود تحقق عشوائي
    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // كود مكون من 6 أرقام
    }

    // دالة لإرسال البريد الإلكتروني
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
            // سجل الخطأ إذا لزم الأمر
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }

    // واجهة التسجيل
    public IActionResult Index()
    {
        ViewData["HideNavbar"] = true;
        ViewData["HideFooter"] = true;
        return View();
    }

    // إضافة المستخدم وإرسال كود التحقق
    [HttpPost]
    public IActionResult add(User user)
    {
        if (_chatDbcontect.users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "هذا البريد الإلكتروني مسجل بالفعل.");
            return View("Index");
        }

        // إنشاء كود تحقق
        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiry = DateTime.Now.AddMinutes(10); // صلاحية الكود لمدة 10 دقائق
        user.IsVerified = false;

        HttpContext.Session.Set("UserStep1", user);

        // إرسال كود التحقق إلى البريد الإلكتروني
        var emailBody = $"Your verification code is: <strong>{verificationCode}</strong>";
        SendEmail(user.Email, "Verify your email", emailBody);

        return RedirectToAction("Addphoto");
    }

    // واجهة إضافة الصورة
    public IActionResult Addphoto()
    {
        ViewData["HideNavbar"] = true;
        ViewData["HideFooter"] = true;
        return View();
    }

    // إنهاء التسجيل وإضافة الصورة
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

       

        if (ProfilePicture != null && ProfilePicture.Length > 0)
        {
            var filePath = Path.Combine("wwwroot/uploads", ProfilePicture.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePicture.CopyToAsync(stream);
            }
            user.ProfilePicture = $"/uploads/{ProfilePicture.FileName}";
        }

        _chatDbcontect.users.Add(user);
        await _chatDbcontect.SaveChangesAsync();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.Now.AddDays(30)
        };

        Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);

        return RedirectToAction("VerifyEmail");
    }

    // واجهة إدخال كود التحقق
    public IActionResult VerifyEmail()
    {
        ViewData["HideNavbar"] = true;
        ViewData["HideFooter"] = true;
        return View();
    }

    // التحقق من الكود
    [HttpPost]
    public IActionResult VerifyEmail(string code)
    {
        var user = _chatDbcontect.users.FirstOrDefault(u => u.VerificationCode == code && u.VerificationCodeExpiry > DateTime.Now);
        if (user != null)
        {
            user.IsVerified = true;
            _chatDbcontect.SaveChanges();

            return RedirectToAction("Cooky", "Home");
        }

        ModelState.AddModelError("Code", "الكود غير صحيح أو منتهي الصلاحية.");
        return View();
    }
}