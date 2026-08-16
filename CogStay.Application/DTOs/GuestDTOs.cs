using System;
using System.ComponentModel.DataAnnotations;

namespace CogStay.Application.DTOs;

public class GuestResponseDTO
{
    public int GuestId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGuestDTO
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500)]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = null!;
}

public class UpdateGuestDTO
{
    public int GuestId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(500)]
    public string Address { get; set; } = null!;
}

public class GuestLoginDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;
}
