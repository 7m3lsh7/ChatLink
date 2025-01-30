using Microsoft.AspNetCore.SignalR;
using System.Net.Mail;
using System.Net;
using webchat.data;
using webchat.Models;
using Microsoft.Extensions.Configuration;

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
    private static Dictionary<int, string> _userConnections = new Dictionary<int, string>();

    // Method to handle sending a message from the sender to the receiver
    public async Task SendMessage(int SenderId, int ReceiverId, string Content)
    {
        // Creating a new Chat message object to store in the database
        var newMessage = new Chat
        {
            SenderId = SenderId,
            ReceiverId = ReceiverId,
            Content = Content,
            Timestamp = DateTime.UtcNow  // Set the timestamp of when the message was sent
        };

        // Add the new message to the database
        _context.chats.Add(newMessage);
        await _context.SaveChangesAsync();  // Save changes to the database

        // Check if the receiver is currently online (has an active connection)
        var connectionId = _userConnections.ContainsKey(ReceiverId) ? _userConnections[ReceiverId] : null;

        if (connectionId != null)
        {
            // If the receiver is online, send the message to them via SignalR
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", SenderId, Content);
        }
        else
        {
            // If the receiver is offline, send an email notification
            await SendEmailNotification(ReceiverId, Content);
        }

        // Inform the sender that their message was successfully sent
        await Clients.User(SenderId.ToString()).SendAsync("MessageSent", Content);
    }

    // Override method that is called when a user connects to the SignalR hub
    public override Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];  // Retrieve the user's ID from cookies

        if (!string.IsNullOrEmpty(userId))
        {
            // Add the user to the online connections tracking dictionary
            _userConnections[int.Parse(userId)] = Context.ConnectionId;
            Groups.AddToGroupAsync(Context.ConnectionId, userId);  // Add user to a SignalR group for receiving messages
        }

        // Notify other connected users that this user is now online
        Clients.Others.SendAsync("UserOnline", userId);
        return base.OnConnectedAsync();
    }

    // Override method that is called when a user disconnects from the SignalR hub
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];  // Retrieve the user's ID from cookies

        if (!string.IsNullOrEmpty(userId))
        {
            // Remove the user from the online connections tracking dictionary
            _userConnections.Remove(int.Parse(userId));
            Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);  // Remove user from the SignalR group
        }

        // Notify other connected users that this user is now offline
        Clients.Others.SendAsync("UserOffline", userId);
        return base.OnDisconnectedAsync(exception);
    }

    // Method to send an email notification to the receiver if they are offline
    private async Task SendEmailNotification(int receiverId, string message)
    {
        try
        {
            // Get the receiver's email from the database based on their user ID
            var receiverEmail = GetUserEmail(receiverId);
            var subject = "📩 You have a new message waiting for you!"; // Email subject
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
                    "; // Email body content

            // Retrieve SMTP settings from configuration
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpUserName = _configuration["SmtpSettings:UserName"];
            var smtpPassword = _configuration["SmtpSettings:Password"];
            var smtpFrom = _configuration["SmtpSettings:From"];
            var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]);

            // Configure the SMTP client to send the email
            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUserName, smtpPassword),
                EnableSsl = enableSsl
            };

            // Create and configure the email message
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpFrom),
                Subject = subject,
                Body = body,
                IsBodyHtml = false  // Set to false for plain text email
            };

            mailMessage.To.Add(receiverEmail);  // Add the receiver's email address

            // Send the email asynchronously
            await smtpClient.SendMailAsync(mailMessage);

            Console.WriteLine("Email sent successfully");  // Log successful email sending
        }
        catch (Exception ex)
        {
            // Log any error that occurs while sending the email
            Console.WriteLine($"Error sending email: {ex.Message}");
        }
    }

    // Method to retrieve the email address of a user based on their userId
    private string GetUserEmail(int userId)
    {
        var user = _context.users.FirstOrDefault(u => u.Id == userId);  // Find the user by their ID

        // Return the user's email if found, or a default email if not found
        return user?.Email ?? "default@example.com";
    }
}
