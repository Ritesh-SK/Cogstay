using Microsoft.EntityFrameworkCore;
using CogStayMVC.Data;
using CogStayMVC.Models;
using CogStayMVC.Enums;
using CogStayMVC.Repositories.Implementations;
using CogStayMVC.Repositories.Interfaces;
using CogStayMVC.Services.Interfaces;
using CogStayMVC.Repositories.Admin;
using CogStayMVC.Repositories.FrontDesk;
using CogStayMVC.Repositories.GuestModule;
using CogStayMVC.Repositories.Housekeeping;
using CogStayMVC.Repositories.Manager;
using CogStayMVC.Services.Admin;
using CogStayMVC.Services.FrontDesk;
using CogStayMVC.Services.GuestModule;
using CogStayMVC.Services.Housekeeping;
using CogStayMVC.Services.Manager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddControllersAsServices();

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Repositories
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IStayRecordRepository, StayRecordRepository>();
builder.Services.AddScoped<IBillingRepository, BillingRepository>();
builder.Services.AddScoped<IHousekeepingTaskRepository, HousekeepingTaskRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// Register Services
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IHousekeepingService, HousekeepingService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Map API Controllers
app.MapControllers();

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Default Administrator Account
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    var defaultAdminEmail = "admin@gmail.com";
    if (!context.Staff.Any(s => s.Email == defaultAdminEmail))
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("123456"));
        var pwdHash = Convert.ToBase64String(bytes);

        var admin = new Staff
        {
            FullName = "Administrator",
            Email = defaultAdminEmail,
            PhoneNumber = "0000000000",
            PasswordHash = pwdHash,
            Role = StaffRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        context.Staff.Add(admin);
        context.SaveChanges();
    }
}

app.Run();
