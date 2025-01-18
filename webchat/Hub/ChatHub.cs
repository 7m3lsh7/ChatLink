using Microsoft.AspNetCore.SignalR;
using webchat.data;
using webchat.Models;

public class ChatHub : Hub
{
    private readonly ChatDbcontect _context;

    public ChatHub(ChatDbcontect context)
    {
        _context = context;
    }
    public async Task SendMessage(string senderId, string receiverId, string message)
    {
        var newMessage = new Chat
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = message,
            Timestamp = DateTime.UtcNow
        };

        _context.chats.Add(newMessage);
        await _context.SaveChangesAsync();


        await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
    }
 }