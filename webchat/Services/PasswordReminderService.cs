using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using webchat.data;
using webchat.Models;

public class PasswordReminderService
{
    private readonly ChatDbcontect _chatDbContext;

    public PasswordReminderService(ChatDbcontect chatDbContext)
    {
        _chatDbContext = chatDbContext;
    }

    public async Task SendPasswordReminderEmails()
    {
        var usersToNotify = _chatDbContext.users
            .AsNoTracking()
            .Where(u => u.LastPasswordChangeDate < DateTime.Now.AddMonths(-3)) // على سبيل المثال، أكثر من 3 أشهر
            .ToList();

        foreach (var user in usersToNotify)
        {
            string subject = "تذكير بتحديث كلمة المرور";
            string websiteLink = "https://chatlink.runasp.net/"; // رابط موقعك
            string body = $@"
                <html>
                <body>
                    <h1>مرحبًا {user.Username}!</h1>
                    <p>لاحظنا أنك لم تقم بتحديث كلمة المرور الخاصة بك منذ فترة. من أجل الحفاظ على أمان حسابك، ننصحك بتحديث كلمة المرور في أقرب وقت ممكن.</p>
                    <p>يمكنك تسجيل الدخول وتحديث كلمة المرور من خلال الرابط التالي:</p>
                    <p><a href='{websiteLink}'>{websiteLink}</a></p>
                    <p>إذا كانت لديك أي أسئلة، فلا تتردد في التواصل معنا.</p>
                    <p>مع خالص التقدير،<br/>فريق الدعم</p>
                </body>
                </html>";

            // تحقق من إذا كانت كلمة المرور مشفرة باستخدام bcrypt
            if (!IsPasswordHashed(user.PasswordHash))
            {
                // إذا كانت كلمة المرور غير مشفرة، أرسل له رسالة لتحديث كلمة المرور
                string reminderBody = $@"
                    <html>
                    <body>
                        <h1>مرحبًا {user.Username}!</h1>
                        <p>لاحظنا أن كلمة المرور الخاصة بك غير مشفرة بشكل آمن. من أجل الحفاظ على أمان حسابك، ننصحك بتغيير كلمة المرور إلى كلمة مرور مشفرة باستخدام معيار bcrypt.</p>
                        <p>يمكنك تسجيل الدخول وتحديث كلمة المرور من خلال الرابط التالي:</p>
                        <p><a href='{websiteLink}'>{websiteLink}</a></p>
                        <p>إذا كانت لديك أي أسئلة، فلا تتردد في التواصل معنا.</p>
                        <p>مع خالص التقدير،<br/>فريق الدعم</p>
                    </body>
                    </html>";
                await SendEmail(user.Email, "تنبيه: كلمة المرور غير مشفرة", reminderBody);
            }
            else
            {
                // إذا كانت كلمة المرور مشفرة، أرسل له رسالة التذكير العادية
                await SendEmail(user.Email, subject, body);
            }
        }
    }

    private bool IsPasswordHashed(string password)
    {
        // تحقق من إذا كانت كلمة المرور تبدأ بـ "$2a$" وهو التنسيق الشائع لـ bcrypt
        return password.StartsWith("$2a$");
    }

    private async Task SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            MailMessage mail = new MailMessage("ChatLink.eg@gmail.com", toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new System.Net.NetworkCredential("ChatLink.eg@gmail.com", "ffhm glyt bcup ddke");
            smtp.EnableSsl = true;

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
}
