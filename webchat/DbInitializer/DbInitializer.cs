using webchat.data;
using webchat.Models;
using System.Linq;

public class DbInitializer
{
    private readonly ChatDbcontect _chatDbContext;

    public DbInitializer(ChatDbcontect chatDbContext)
    {
        _chatDbContext = chatDbContext;
    }

    public void Initialize()
    {
        var usersToUpdate = _chatDbContext.users 
            .Where(u => u.IsPasswordChanged == false && u.LastPasswordChangeDate == null)
            .ToList();

        foreach (var user in usersToUpdate)
        {
            user.IsPasswordChanged = false; 
            user.LastPasswordChangeDate = null;
            _chatDbContext.users.Update(user);
        }

        _chatDbContext.SaveChanges();
    }
}
