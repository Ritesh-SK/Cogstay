using System;
using System.ComponentModel.DataAnnotations;

namespace CogStay.Application.DTOs;

public class StayRecordResponseDTO
{
    public int StayId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = null!;
    public int ReservationId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public bool IsCheckedOut => ActualCheckOut.HasValue;
    public BillingResponseDTO? Billing { get; set; }

    public string? BookingReference { get; set; }
    public string? BillingReference { get; set; }
    public string? StayDetails { get; set; }
}

public class CreateCheckInDTO
{
    [Required(ErrorMessage = "Reservation ID is required.")]
    public int ReservationId { get; set; }
}

public class CheckOutDTO
{
    [Required(ErrorMessage = "Stay ID is required.")]
    public int StayId { get; set; }
}
