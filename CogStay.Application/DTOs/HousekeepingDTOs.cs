using System.ComponentModel.DataAnnotations;
using TaskStatus = CogStay.Domain.Enums.TaskStatus;

namespace CogStay.Application.DTOs;

public class HousekeepingTaskResponseDTO
{
    public int TaskId { get; set; }
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string TaskDescription { get; set; } = null!;
    public TaskStatus TaskStatus { get; set; }
}

public class CreateHousekeepingTaskDTO
{
    [Required(ErrorMessage = "Room selection is required.")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Task description is required.")]
    [StringLength(1000)]
    public string TaskDescription { get; set; } = null!;
}

public class UpdateTaskStatusDTO
{
    public int TaskId { get; set; }

    [Required]
    public TaskStatus TaskStatus { get; set; }
}
