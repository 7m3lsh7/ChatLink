using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PasswordReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;

    public PasswordReminderBackgroundService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var passwordReminderService = scope.ServiceProvider.GetRequiredService<PasswordReminderService>();
                    await passwordReminderService.SendPasswordReminderEmails();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PasswordReminderBackgroundService: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}