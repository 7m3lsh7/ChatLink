using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            .AsEnumerable() 
            .Where(u => u.LastPasswordChangeDate < DateTime.Now.AddMonths(-3))
            .ToList();

        foreach (var user in usersToNotify)
        {
            string subject = "Reminder to update your password";
            string body = $@"
            <html>
           <body>
                  <h1>Dear, {user.Username}!</h1>
                  <p>We hope you're doing well. Our security protocols have flagged that it has been over 3 months since your last password update. To ensure the continued safety of your account and to safeguard against potential security risks, we highly recommend updating your password at your earliest convenience.</p>
                  <p>To make the process as seamless as possible, simply log in to your account and update your password via the following link:</p>
                  <p><a href='https://chatlink.runasp.net/'>Click here to update your password</a></p>
                  <p>If you have any questions or require assistance, our dedicated support team is always here to help. Feel free to reach out via email or connect with us through the live chat on our platform.</p>
                  <p>Thank you for choosing <strong>ChatLink</strong>. We're committed to your security and ensuring the best experience possible.</p>
                  <p>Warm regards,<br/>
                  The ChatLink Team</p>
           </body>
            </html>";

            await SendEmail(user.Email, subject, body);
        }

        var usersToNotifyForHashing = _chatDbContext.users
            .AsEnumerable() 
            .Where(u => !IsPasswordHashed(u.PasswordHash)) 
            .ToList();

        foreach (var user in usersToNotifyForHashing)
        {
            string reminderBody = $@"
            <html>
         <body>
             <h1>Hello {user.Username}!</h1>
             <p>Your account security is our top priority. We've noticed that your password is not securely hashed according to modern security standards. To ensure your account remains safe from potential threats, we strongly recommend updating your password to one that is hashed using the advanced bcrypt standard.</p>
             <p>You can log in and update your password through the following link:</p>
             <p><a href='https://chatlink.runasp.net/'>Click here to update your password</a></p>
             <p>If you have any questions, feel free to reach out to us.</p>
             <p>Best regards,<br/>The ChatLink Team</p>
       </body>
            </html>";

            await SendEmail(user.Email, "Warning: The password is not encrypted⚠️", reminderBody);
        }
    }

    private bool IsPasswordHashed(string password)
    {
        return password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$");
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
