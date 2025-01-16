using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    // استقبال الرسالة من العميل
    public async Task SendMessage(string user, string message)
    {
        // إرسال الرسالة لجميع العملاء
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}