using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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
    var baseUrl = builder.Configuration["API_BASE_URL"] 
        ?? builder.Configuration["ApiSettings:BaseUrl"] 
        ?? "https://localhost:5001/";

    // Defensive cleanup of any markdown or trailing characters
    baseUrl = baseUrl.Trim('[', ']', '(', ')', ' ');
    if (baseUrl.Contains("]("))
    {
        var parts = baseUrl.Split("](");
        baseUrl = parts.Length > 1 ? parts[1].TrimEnd(')') : parts[0];
    }
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }

    client.BaseAddress = new Uri(baseUrl);
});

// Configure Forwarded Headers for Render Reverse Proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
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

app.UseForwardedHeaders();

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
