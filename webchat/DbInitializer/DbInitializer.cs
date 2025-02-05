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

    // هذه الدالة ستقوم بتحديث المستخدمين القدامى الذين لم يغيروا كلمة المرور
    public void Initialize()
    {
        // تحديث المستخدمين الذين لم يتم تغيير كلمة المرور لديهم
        var usersToUpdate = _chatDbContext.users 
            .Where(u => u.IsPasswordChanged == false && u.LastPasswordChangeDate == null)
            .ToList();

        foreach (var user in usersToUpdate)
        {
            user.IsPasswordChanged = false; // تعيين IsPasswordChanged لـ false للمستخدمين القدامى
            user.LastPasswordChangeDate = null; // تعيين LastPasswordChangeDate إلى null للمستخدمين القدامى
            _chatDbContext.users.Update(user);
        }

        // حفظ التحديثات في قاعدة البيانات
        _chatDbContext.SaveChanges();
    }
}
