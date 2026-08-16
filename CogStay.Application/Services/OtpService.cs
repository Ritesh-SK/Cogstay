using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;
using CogStay.Domain.Enums;

namespace CogStay.Application.Services;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public OtpService(
        IOtpRepository otpRepository,
        IGuestRepository guestRepository,
        IEmailService emailService,
        ISmsService smsService)
    {
        _otpRepository = otpRepository;
        _guestRepository = guestRepository;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task SendEmailOtpAsync(string userId, string email)
    {
        await GenerateAndSendOtpAsync(userId, email, OtpType.Email);
    }

    public async Task SendPhoneOtpAsync(string userId, string phone)
    {
        await GenerateAndSendOtpAsync(userId, phone, OtpType.Phone);
    }

    public async Task<OtpResultDTO> VerifyEmailOtpAsync(VerifyEmailOtpDTO dto)
    {
        var guest = await _guestRepository.GetByEmailAsync(dto.Email);
        if (guest == null)
        {
            return new OtpResultDTO { Success = false, Message = "Guest email not found." };
        }

        var result = await VerifyOtpAsync(guest.Id, dto.Email, OtpType.Email, dto.Code);
        if (!result.Success) return result;

        guest.EmailVerified = true;
        if (guest.PhoneVerified)
        {
            guest.IsActive = true;
            result.IsAccountActivated = true;
            result.Message = "Email verified successfully! Both Email and Phone are now verified. Account activated.";
        }
        else
        {
            result.Message = "Email verified successfully! Please verify your Phone OTP to activate your account.";
        }

        await _guestRepository.UpdateAsync(guest);
        return result;
    }

    public async Task<OtpResultDTO> VerifyPhoneOtpAsync(VerifyPhoneOtpDTO dto)
    {
        var guest = await _guestRepository.GetByPhoneAsync(dto.PhoneNumber);
        if (guest == null)
        {
            return new OtpResultDTO { Success = false, Message = "Guest phone number not found." };
        }

        var result = await VerifyOtpAsync(guest.Id, dto.PhoneNumber, OtpType.Phone, dto.Code);
        if (!result.Success) return result;

        guest.PhoneVerified = true;
        if (guest.EmailVerified)
        {
            guest.IsActive = true;
            result.IsAccountActivated = true;
            result.Message = "Phone verified successfully! Both Email and Phone are now verified. Account activated.";
        }
        else
        {
            result.Message = "Phone verified successfully! Please verify your Email OTP to activate your account.";
        }

        await _guestRepository.UpdateAsync(guest);
        return result;
    }

    public async Task ResendOtpAsync(ResendOtpDTO dto)
    {
        Guest? guest = null;
        if (dto.OtpType == OtpType.Email)
        {
            guest = await _guestRepository.GetByEmailAsync(dto.Target);
        }
        else
        {
            guest = await _guestRepository.GetByPhoneAsync(dto.Target);
        }

        if (guest == null)
        {
            throw new KeyNotFoundException($"Account not found for target '{dto.Target}'.");
        }

        await GenerateAndSendOtpAsync(guest.Id, dto.Target, dto.OtpType);
    }

    private async Task GenerateAndSendOtpAsync(string userId, string target, OtpType type)
    {
        var existing = await _otpRepository.GetLatestValidOtpAsync(userId, target, type);
        if (existing != null && existing.LastSentAt.AddSeconds(60) > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Please wait 60 seconds before requesting a new OTP.");
        }

        // Invalidate older OTPs for this target/type
        await _otpRepository.InvalidateExistingOtpsAsync(userId, target, type);

        // Generate cryptographically secure 6-digit code
        string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        string codeHash = HashCode(code);

        var otpRecord = new OtpRecord
        {
            UserId = userId,
            Target = target,
            OtpType = type,
            CodeHash = codeHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            AttemptCount = 0,
            LastSentAt = DateTime.UtcNow,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _otpRepository.CreateAsync(otpRecord);

        if (type == OtpType.Email)
        {
            await _emailService.SendEmailAsync(target, "CogStay - Your Email Verification OTP",
                $"Your verification code is: <b>{code}</b>. It is valid for 10 minutes.");
        }
        else
        {
            await _smsService.SendSmsAsync(target, $"CogStay verification code: {code}. Valid for 10 minutes.");
        }
    }

    private async Task<OtpResultDTO> VerifyOtpAsync(string userId, string target, OtpType type, string code)
    {
        var otp = await _otpRepository.GetLatestValidOtpAsync(userId, target, type);
        if (otp == null || otp.IsUsed || otp.ExpiresAt <= DateTime.UtcNow)
        {
            return new OtpResultDTO { Success = false, Message = "Invalid or expired OTP. Please request a new code." };
        }

        if (otp.AttemptCount >= 5)
        {
            otp.IsUsed = true; // Lock out this OTP
            await _otpRepository.UpdateAsync(otp);
            return new OtpResultDTO { Success = false, Message = "Maximum verification attempts exceeded. Please request a new OTP." };
        }

        otp.AttemptCount++;

        string inputHash = HashCode(code);
        if (inputHash != otp.CodeHash)
        {
            await _otpRepository.UpdateAsync(otp);
            int remaining = 5 - otp.AttemptCount;
            return new OtpResultDTO { Success = false, Message = $"Incorrect OTP. {remaining} attempt(s) remaining." };
        }

        otp.IsUsed = true;
        await _otpRepository.UpdateAsync(otp);

        return new OtpResultDTO { Success = true, Message = "OTP verified successfully." };
    }

    private static string HashCode(string code)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}
