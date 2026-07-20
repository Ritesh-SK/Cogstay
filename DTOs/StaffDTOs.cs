using System;
using System.ComponentModel.DataAnnotations;
using CogStayMVC.Enums;

namespace CogStayMVC.DTOs;

public class StaffResponseDTO
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public StaffRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStaffDTO
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Staff Role is required.")]
    public StaffRole Role { get; set; }
}

public class UpdateStaffDTO
{
    public int StaffId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public StaffRole Role { get; set; }

    public bool IsActive { get; set; }
}

public class StaffLoginDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role is required.")]
    public StaffRole Role { get; set; }
}
