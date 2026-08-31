using System.Collections.Generic;
using System.Threading.Tasks;
using CogStay.Application.DTOs;

namespace CogStay.Application.Contracts.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDTO>> GetUserNotificationsAsync(string targetUserId, string targetRole, int limit = 10);
    Task<UnreadCountDTO> GetUnreadCountAsync(string targetUserId, string targetRole);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string targetUserId, string targetRole);
    Task CreateNotificationAsync(CreateNotificationDTO dto);
}
