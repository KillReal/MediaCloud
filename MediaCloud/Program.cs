using BenchmarkDotNet.Running;
using MediaCloud.Data;
using MediaCloud.Repositories;
using MediaCloud.Services;
using MediaCloud.MediaUploader;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

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

var app = builder.Build();

using var scope = app.Services.CreateScope();

PictureService.PictureServicelazyInit(scope.ServiceProvider.GetRequiredService<IConfiguration>());
Uploader.Init(scope.ServiceProvider.GetRequiredService<AppDbContext>());


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
