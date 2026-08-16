using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CogStay.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add MVC Services
builder.Services.AddControllersWithViews();

// Register Shared Infrastructure & Application Services
builder.Services.AddInfrastructureAndApplication();

// Register HttpClient for CogStayApi
builder.Services.AddHttpClient("CogStayApi", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] 
        ?? builder.Configuration["API_BASE_URL"] 
        ?? "https://localhost:5001/";
    client.BaseAddress = new Uri(baseUrl);
});

// Session & Cookie Configuration for Secure Token Handling
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
