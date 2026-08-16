using System.ComponentModel.DataAnnotations;
using CogStay.Domain.Enums;

namespace CogStay.Application.DTOs;

public class VerifyEmailOtpDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
    public string Code { get; set; } = null!;
}

public class VerifyPhoneOtpDTO
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
    public string Code { get; set; } = null!;
}

public class ResendOtpDTO
{
    [Required]
    public string Target { get; set; } = null!; // Email or Phone number

    [Required]
    public OtpType OtpType { get; set; }
}

public class OtpResultDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public bool IsAccountActivated { get; set; }
}
