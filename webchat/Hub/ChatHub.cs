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

    public async Task SendMessage(int SenderId,  int ReceiverId, string Content)
    {
         var newMessage = new Chat
        {
            SenderId = SenderId,
            ReceiverId = ReceiverId,
            Content = Content,
            Timestamp = DateTime.UtcNow
        };

        _context.chats.Add(newMessage);

        await _context.SaveChangesAsync();
 
         await Clients.Group(ReceiverId.ToString()).SendAsync("ReceiveMessage", SenderId, Content);
        await Clients.User(ReceiverId.ToString()).SendAsync("NewMessageNotification");


        await Clients.User(SenderId.ToString()).SendAsync("MessageSent", Content);
    }
    public override Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];
        if (!string.IsNullOrEmpty(userId))
        {
            Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} added to group");

        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Cookies["UserId"];
        if (!string.IsNullOrEmpty(userId))
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        return base.OnDisconnectedAsync(exception);
    }

}
