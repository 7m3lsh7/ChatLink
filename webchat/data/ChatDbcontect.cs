using Microsoft.EntityFrameworkCore;
using webchat.Models;

namespace webchat.data
{
    public class ChatDbcontect :  DbContext
    {
        public ChatDbcontect(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> users { get; set; }
        public DbSet<Chat> chats { get; set; }
        public DbSet<ContactModel> contacts { get; set; }

    }
}
