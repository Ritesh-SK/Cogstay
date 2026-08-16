using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;

namespace CogStay.Infrastructure.Services;

public class IdempotentAdminSeeder
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher<Staff> _passwordHasher;
    private readonly IConfiguration _configuration;

    public IdempotentAdminSeeder(
        IStaffRepository staffRepository,
        IPasswordHasher<Staff> passwordHasher,
        IConfiguration configuration)
    {
        _staffRepository = staffRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var adminEmail = _configuration["ADMIN_SEED_EMAIL"] 
            ?? _configuration["AdminSeed:Email"] 
            ?? "admin@gmail.com";

        var adminPassword = _configuration["ADMIN_SEED_PASSWORD"] 
            ?? _configuration["AdminSeed:Password"] 
            ?? "123456";

        var existing = await _staffRepository.GetByEmailAsync(adminEmail);
        if (existing != null)
        {
            // Idempotent: Admin account already exists, do not recreate or overwrite
            return;
        }

        var nextId = await _staffRepository.GetNextStaffIdAsync();
        var admin = new Staff
        {
            StaffId = nextId,
            FullName = "System Administrator",
            Email = adminEmail,
            PhoneNumber = "0000000000",
            Role = StaffRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, adminPassword);
        await _staffRepository.CreateAsync(admin);
        Console.WriteLine($"[IdempotentAdminSeeder] Admin account '{adminEmail}' seeded successfully.");
    }
}
