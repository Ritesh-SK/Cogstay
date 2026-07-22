using System;
using CogStayMVC.Enums;

namespace CogStayMVC.Models;

public class Reservation
{
    public int ReservationId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus ReservationStatus { get; set; }

    // Navigation Properties
    public virtual Guest Guest { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
    public virtual StayRecord? StayRecord { get; set; }
}
