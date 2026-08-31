using System.Collections.Generic;
using System.Threading.Tasks;
using CogStay.Domain.Entities;

namespace CogStay.Application.Contracts.Persistence;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string targetUserId, string targetRole, int limit = 10);
    Task<int> GetUnreadCountAsync(string targetUserId, string targetRole);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string targetUserId, string targetRole);
    Task CreateAsync(Notification notification);
    Task SeedInitialNotificationsIfEmptyAsync();
}
