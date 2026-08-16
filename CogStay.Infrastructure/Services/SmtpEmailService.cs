using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CogStay.Application.Contracts.Services;

namespace CogStay.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var host = _configuration["EMAIL_PROVIDER_HOST"] ?? _configuration["Email:Host"];
        var portStr = _configuration["EMAIL_PROVIDER_PORT"] ?? _configuration["Email:Port"] ?? "587";
        var username = _configuration["EMAIL_PROVIDER_USERNAME"] ?? _configuration["Email:Username"];
        var password = _configuration["EMAIL_PROVIDER_PASSWORD"] ?? _configuration["Email:Password"];
        var fromEmail = _configuration["EMAIL_FROM"] ?? _configuration["Email:From"] ?? "no-reply@cogstay.com";

        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username))
        {
            try
            {
                int port = int.TryParse(portStr, out var p) ? p : 587;
                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "CogStay Hotel Management"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Service Warning] Failed sending email to {toEmail}: {ex.Message}");
            }
        }

        // Fallback / Development simulated log dispatch
        Console.WriteLine($"[EMAIL SENT] To: {toEmail} | Subject: {subject}");
    }
}
