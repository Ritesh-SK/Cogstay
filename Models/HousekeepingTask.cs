using CogStayMVC.Enums;

namespace CogStayMVC.Models;

public class HousekeepingTask
{
    public int TaskId { get; set; }
    public int RoomId { get; set; }
    public string TaskDescription { get; set; } = null!;
    public CogStayMVC.Enums.TaskStatus TaskStatus { get; set; }

    // Navigation Properties
    public virtual Room Room { get; set; } = null!;
}
