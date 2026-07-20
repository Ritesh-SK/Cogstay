using System;

namespace CogStayMVC.Models;

public class Feedback
{
    public int FeedbackId { get; set; }
    public int GuestId { get; set; }
    public int? ReservationId { get; set; }
    public int Rating { get; set; } // 1 to 5
    public string Comments { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    public virtual Guest Guest { get; set; } = null!;
    public virtual Reservation? Reservation { get; set; }
}
