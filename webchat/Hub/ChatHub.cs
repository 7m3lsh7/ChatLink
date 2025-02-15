using Microsoft.AspNetCore.SignalR;
using System.Net.Mail;
using System.Net;
using webchat.data;
using webchat.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

public class ChatHub : Hub
{
    private readonly ChatDbcontect _context;
    private readonly IConfiguration _configuration;

    // Constructor to inject dependencies such as database context and configuration settings
    public ChatHub(ChatDbcontect context, IConfiguration configuration)
    {
        _context = context;  // Assigning the database context for CRUD operations on chat data
        _configuration = configuration;  // Injecting configuration for SMTP settings
    }

    // A static dictionary to keep track of user connections (userId and connectionId)
    private static ConcurrentDictionary<int, string> _userConnections = new ConcurrentDictionary<int, string>();

    public async Task SendMessage(int SenderId, int ReceiverId, string Content, string messageType)
    {
        try
        {
            Console.WriteLine($"[INFO] SendMessage received - SenderId: {SenderId}, ReceiverId: {ReceiverId}, Content: '{Content}', MessageType: '{messageType}'");

            var newMessage = new Chat
            {
                SenderId = SenderId,
                ReceiverId = ReceiverId,
                Content = Content,
                MessageType = messageType,
                Timestamp = DateTime.UtcNow
            };

            _context.chats.Add(newMessage);
            await _context.SaveChangesAsync();
            Console.WriteLine("[INFO] Message saved in DB successfully.");

            var connectionId = _userConnections.ContainsKey(ReceiverId) ? _userConnections[ReceiverId] : null;

            if (connectionId != null)
            {
                Console.WriteLine($"[INFO] Sending message to receiver {ReceiverId}");
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", SenderId, Content, messageType);
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", SenderId, "New message received");
                Console.WriteLine("[INFO] Message sent to receiver successfully.");
            }
            else
            {
                Console.WriteLine($"[WARNING] Receiver {ReceiverId} is offline, sending email notification.");
                await SendEmailNotification(ReceiverId, Content);
            }

            await Clients.User(SenderId.ToString()).SendAsync("MessageSent", Content);
            Console.WriteLine("[INFO] MessageSent event triggered for sender.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to send message: {ex.Message}");
            Console.WriteLine($"[STACK TRACE] {ex.StackTrace}");
            throw;
        }
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];

        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
        {
            _userConnections[userIdInt] = Context.ConnectionId;
            Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Clients.Others.SendAsync("UserOnline", userIdInt);
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];

        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
        {
            _userConnections.TryRemove(userIdInt, out _);
            Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Clients.Others.SendAsync("UserOffline", userIdInt);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private async Task SendEmailNotification(int receiverId, string message)
    {
        try
        {
            var receiverEmail = GetUserEmail(receiverId);
            var subject = "📩 You have a new message waiting for you!";
            var body = $@"
                        Hello,

                        You have received a new message on ChatLink! 🎉

                        Here are the details:
                        - Message Content: {message}
                        - Sent At: {DateTime.Now.ToString("f")} 

                        To view and respond to the message, click the link below:
                        [Go to Your Messages](https://chatlink.runasp.net/Home/Chat)

                        Don't miss out on the conversation! Stay connected and enjoy using ChatLink.

                        Best Regards,  
                        The ChatLink Team 🚀  
                        https://chatlink.runasp.net/Home/Index
                    ";

            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpUserName = _configuration["SmtpSettings:UserName"];
            var smtpPassword = _configuration["SmtpSettings:Password"];
            var smtpFrom = _configuration["SmtpSettings:From"];
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]);

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUserName, smtpPassword),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpFrom),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(receiverEmail);

            await smtpClient.SendMailAsync(mailMessage);
            Console.WriteLine("Email sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }

    private string GetUserEmail(int userId)
    {
        var user = _context.users.FirstOrDefault(u => u.Id == userId);
        return user?.Email ?? "default@example.com";
    }
}
