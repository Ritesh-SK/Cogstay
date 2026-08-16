using System;
using System.ComponentModel.DataAnnotations;
using CogStay.Domain.Enums;

namespace CogStay.Application.DTOs;

public class ReservationResponseDTO
{
    public int ReservationId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = null!;
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string RoomType { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
    public int TotalNights => (CheckOutDate - CheckInDate).Days > 0 ? (CheckOutDate - CheckInDate).Days : 1;
    public decimal EstimatedTotalCost => TotalNights * PricePerNight;
}

public class CreateReservationDTO
{
    [Required(ErrorMessage = "Guest selection is required.")]
    public int GuestId { get; set; }

    [Required(ErrorMessage = "Room selection is required.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Check-in date is required.")]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Check-out date is required.")]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);
}

public class UpdateReservationDTO
{
    public int ReservationId { get; set; }

    [Required]
    public int GuestId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; }

    public ReservationStatus ReservationStatus { get; set; }
}
