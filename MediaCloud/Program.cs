using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.TaskScheduler;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MediaCloud.WebApp.Services.Statistic;
using MediaCloud.WebApp.Services.UserProvider;
using NLog.Web;
using NLog;
using MediaCloud.WebApp.Services.ConfigProvider;
using MediaCloud.WebApp;
using MediaCloud.WebApp.Services.AutotagService;
using Npgsql;
using MediaCloud.WebApp.Repositories;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Early NLog initialization");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddNLogWeb();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options => 
{
    _ = options.UseNpgsql(builder.Configuration.GetConnectionString("Database") ?? throw new NpgsqlException("Database connection string must be specified"));
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IConfigProvider, ConfigProvider>();
builder.Services.AddSingleton<IUserProvider, UserProvider>();
builder.Services.AddSingleton<ITaskScheduler, MediaCloud.TaskScheduler.TaskScheduler>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IAutotagService, AutotagService>();
builder.Services.AddSingleton<IPictureService, PictureService>();
builder.Services.AddSingleton<StatisticProvider>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PreviewRepository>();
builder.Services.AddScoped<BlobRepository>();
builder.Services.AddScoped<CollectionRepository>();
builder.Services.AddScoped<TagRepository>();

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => 
    {
        options.LoginPath = "/User/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Security:CookieExpireTime"));
    });
builder.Services.AddAuthorization();

var app = builder.Build();
//using var scope = app.Services.CreateScope();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
