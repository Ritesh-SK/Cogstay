using System.Collections.Generic;
using CogStayMVC.Enums;

namespace CogStayMVC.Models;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string RoomType { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public RoomStatus Status { get; set; }

    // Navigation Properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<HousekeepingTask> HousekeepingTasks { get; set; } = new List<HousekeepingTask>();
}
