using System;
using System.ComponentModel.DataAnnotations;
using CogStay.Domain.Enums;

namespace CogStay.Application.DTOs;

public class LoginRequestDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; } = null!;
    public int IntegerId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IsActive { get; set; }
}

public class RefreshTokenRequestDTO
{
    [Required]
    public string AccessToken { get; set; } = null!;

    [Required]
    public string RefreshToken { get; set; } = null!;
}

public class RegisterResponseDTO
{
    public int GuestId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = "Registration successful. Please verify both Email OTP and Phone OTP to activate your account.";
}
