using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// 生产环境从环境变量读取连接字符串
if (builder.Environment.IsProduction())
{
    var connStr = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection") 
                  ?? builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connStr))
        throw new InvalidOperationException("生产环境必须设置数据库连接字符串环境变量 SQLCONNSTR_DefaultConnection");
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<KnowledgeStack.Web.Models.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsProduction() 
            ? Microsoft.AspNetCore.Http.CookieSecurePolicy.Always 
            : Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddScoped<KnowledgeStack.Web.Services.IAuthService, KnowledgeStack.Web.Services.AuthService>();

// 生产环境：添加 HTTPS 重定向和安全头
if (builder.Environment.IsProduction())
{
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 生产环境安全头
if (app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
