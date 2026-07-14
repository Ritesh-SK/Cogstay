using System;
using System.Collections.Generic;

namespace CogStayMVC.Models;

public class Guest
{
    public int GuestId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<StayRecord> StayRecords { get; set; } = new List<StayRecord>();
}
