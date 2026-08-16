using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.Services;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;
using CogStay.Infrastructure.Repositories;
using CogStay.Infrastructure.Services;

namespace CogStay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureAndApplication(this IServiceCollection services)
    {
        // Register MongoDbContext
        services.AddSingleton<MongoDbContext>();

        // Register Repositories
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IStayRecordRepository, StayRecordRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IHousekeepingTaskRepository, HousekeepingTaskRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register Password Hashers
        services.AddScoped<IPasswordHasher<Guest>, PasswordHasher<Guest>>();
        services.AddScoped<IPasswordHasher<Staff>, PasswordHasher<Staff>>();

        // Register External Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISmsService, ConsoleSmsService>();
        services.AddScoped<IdempotentAdminSeeder>();

        // Register Application Services
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IHousekeepingService, HousekeepingService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();

        return services;
    }
}
