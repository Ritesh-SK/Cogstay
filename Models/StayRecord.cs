using System;

namespace CogStayMVC.Models;

public class StayRecord
{
    public int StayId { get; set; }
    public int GuestId { get; set; }
    public int ReservationId { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }

    public string GuestName { get; set; } = string.Empty;
    public string? BookingReference { get; set; }
    public string? BillingReference { get; set; }
    public string? StayDetails { get; set; }

    // Navigation Properties
    public virtual Guest Guest { get; set; } = null!;
    public virtual Reservation Reservation { get; set; } = null!;
    public virtual Billing? Billing { get; set; }
}
