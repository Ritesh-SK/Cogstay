using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CogStay.Application.Contracts.Services;

namespace CogStay.Infrastructure.Services;

public class ConsoleSmsService : ISmsService
{
    private readonly IConfiguration _configuration;

    public ConsoleSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        var apiKey = _configuration["SMS_PROVIDER_API_KEY"] ?? _configuration["Sms:ApiKey"];
        var fromNumber = _configuration["SMS_FROM"] ?? _configuration["Sms:From"] ?? "CogStaySMS";

        // Redact OTP or sensitive payload when logging
        Console.WriteLine($"[SMS DISPATCHED] To: {phoneNumber} | Sender: {fromNumber}");
        return Task.CompletedTask;
    }
}
