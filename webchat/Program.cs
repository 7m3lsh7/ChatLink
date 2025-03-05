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

builder.Services.AddDataProtection();

builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));


builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "SessionCache";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
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

app.Use(async (context, next) =>
{
context.Response.Headers.Remove("Server");
context.Response.Headers.Remove("X-Powered-By");
    await next();
});

app.Use(async (context, next) =>
{
    var cookieName = "p9q8r7s6_t34w2x1"; 

    if (context.Request.Cookies[cookieName] != null)
    {
        var encryptedUserId = context.Request.Cookies[cookieName];

        var updatedCookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(30), 
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        };

        context.Response.Cookies.Append(cookieName, encryptedUserId, updatedCookieOptions);
    }

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseCors("CorsPolicy");


app.UseAuthorization();
app.MapHub<ChatHub>("/ChatHub");

app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/{0}");



app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Cooky}/{id?}");

app.Run();
