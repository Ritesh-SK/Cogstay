using System;

namespace CogStay.Application.DTOs;

public class NotificationResponseDTO
{
    public string Id { get; set; } = null!;
    public int NotificationId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "info";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = null!;
}

public class UnreadCountDTO
{
    public int UnreadCount { get; set; }
}

public class CreateNotificationDTO
{
    public string TargetUserId { get; set; } = "All";
    public string TargetRole { get; set; } = "All";
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "info";
}
