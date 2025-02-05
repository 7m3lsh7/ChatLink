using AspNetCore.ReCaptcha;
using Microsoft.EntityFrameworkCore;
using webchat.data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddDbContext<ChatDbcontect>(options => options.UseSqlServer
(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PasswordReminderService>();

builder.Services.AddHostedService<PasswordReminderBackgroundService>();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<DbInitializer>();


builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;  
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();
app.MapHub<ChatHub>("/ChatHub");

app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/{0}");


app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Cooky}/{id?}");

app.Run();
