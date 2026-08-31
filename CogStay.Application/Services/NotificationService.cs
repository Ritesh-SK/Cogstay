using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CogStay.Application.Contracts.Persistence;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;
using CogStay.Domain.Entities;

namespace CogStay.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<NotificationResponseDTO>> GetUserNotificationsAsync(string targetUserId, string targetRole, int limit = 10)
    {
        var list = await _repository.GetUserNotificationsAsync(targetUserId, targetRole, limit);
        return list.Select(n => new NotificationResponseDTO
        {
            Id = n.Id,
            NotificationId = n.NotificationId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            TimeAgo = GetTimeAgo(n.CreatedAt)
        });
    }

    public async Task<UnreadCountDTO> GetUnreadCountAsync(string targetUserId, string targetRole)
    {
        int count = await _repository.GetUnreadCountAsync(targetUserId, targetRole);
        return new UnreadCountDTO { UnreadCount = count };
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        await _repository.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(string targetUserId, string targetRole)
    {
        await _repository.MarkAllAsReadAsync(targetUserId, targetRole);
    }

    public async Task CreateNotificationAsync(CreateNotificationDTO dto)
    {
        var notification = new Notification
        {
            TargetUserId = dto.TargetUserId,
            TargetRole = dto.TargetRole,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(notification);
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var ts = DateTime.UtcNow - dateTime;
        if (ts.TotalSeconds < 60) return "Just now";
        if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m ago";
        if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h ago";
        if (ts.TotalDays < 7) return $"{(int)ts.TotalDays}d ago";
        return dateTime.ToString("MMM dd");
    }
}
