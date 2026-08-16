using System.ComponentModel.DataAnnotations;
using CogStay.Domain.Enums;

namespace CogStay.Application.DTOs;

public class RoomResponseDTO
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string RoomType { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public RoomStatus Status { get; set; }
}

public class CreateRoomDTO
{
    [Required(ErrorMessage = "Room number is required.")]
    [StringLength(50)]
    public string RoomNumber { get; set; } = null!;

    [Required(ErrorMessage = "Room type is required.")]
    [StringLength(100)]
    public string RoomType { get; set; } = null!;

    [Required(ErrorMessage = "Price per night is required.")]
    [Range(0.01, 100000.00, ErrorMessage = "Price per night must be greater than zero.")]
    public decimal PricePerNight { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Available;
}

public class UpdateRoomDTO
{
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Room number is required.")]
    [StringLength(50)]
    public string RoomNumber { get; set; } = null!;

    [Required(ErrorMessage = "Room type is required.")]
    [StringLength(100)]
    public string RoomType { get; set; } = null!;

    [Required(ErrorMessage = "Price per night is required.")]
    [Range(0.01, 100000.00, ErrorMessage = "Price per night must be greater than zero.")]
    public decimal PricePerNight { get; set; }

    public RoomStatus Status { get; set; }
}
