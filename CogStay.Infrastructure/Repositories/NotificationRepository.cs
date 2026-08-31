using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CogStay.Application.Contracts.Persistence;
using CogStay.Domain.Entities;
using CogStay.Infrastructure.Data;

namespace CogStay.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly MongoDbContext _context;

    public NotificationRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string targetUserId, string targetRole, int limit = 10)
    {
        await SeedInitialNotificationsIfEmptyAsync();

        var filter = Builders<Notification>.Filter.Or(
            Builders<Notification>.Filter.Eq(n => n.TargetUserId, targetUserId),
            Builders<Notification>.Filter.Eq(n => n.TargetUserId, "All"),
            Builders<Notification>.Filter.Eq(n => n.TargetRole, targetRole),
            Builders<Notification>.Filter.Eq(n => n.TargetRole, "All")
        );

        return await _context.Notifications
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(string targetUserId, string targetRole)
    {
        await SeedInitialNotificationsIfEmptyAsync();

        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.IsRead, false),
            Builders<Notification>.Filter.Or(
                Builders<Notification>.Filter.Eq(n => n.TargetUserId, targetUserId),
                Builders<Notification>.Filter.Eq(n => n.TargetUserId, "All"),
                Builders<Notification>.Filter.Eq(n => n.TargetRole, targetRole),
                Builders<Notification>.Filter.Eq(n => n.TargetRole, "All")
            )
        );

        return (int)await _context.Notifications.CountDocumentsAsync(filter);
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _context.Notifications.UpdateOneAsync(filter, update);
    }

    public async Task MarkAllAsReadAsync(string targetUserId, string targetRole)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.IsRead, false),
            Builders<Notification>.Filter.Or(
                Builders<Notification>.Filter.Eq(n => n.TargetUserId, targetUserId),
                Builders<Notification>.Filter.Eq(n => n.TargetUserId, "All"),
                Builders<Notification>.Filter.Eq(n => n.TargetRole, targetRole),
                Builders<Notification>.Filter.Eq(n => n.TargetRole, "All")
            )
        );

        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _context.Notifications.UpdateManyAsync(filter, update);
    }

    public async Task CreateAsync(Notification notification)
    {
        await _context.Notifications.InsertOneAsync(notification);
    }

    public async Task SeedInitialNotificationsIfEmptyAsync()
    {
        var count = await _context.Notifications.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var initial = new List<Notification>
            {
                new Notification
                {
                    NotificationId = 1,
                    TargetRole = "Guest",
                    TargetUserId = "All",
                    Title = "Welcome to CogStay",
                    Message = "Thank you for choosing CogStay. Manage your stay, book amenities, and view bills in real time.",
                    Type = "info",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new Notification
                {
                    NotificationId = 2,
                    TargetRole = "Guest",
                    TargetUserId = "All",
                    Title = "Dual OTP Verified",
                    Message = "Your email and phone numbers are verified. Enjoy instant reservation confirmations.",
                    Type = "success",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Notification
                {
                    NotificationId = 3,
                    TargetRole = "Admin",
                    TargetUserId = "All",
                    Title = "System Health Optimal",
                    Message = "MongoDB cluster connected, 0 active database locks, JWT security active.",
                    Type = "system",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new Notification
                {
                    NotificationId = 4,
                    TargetRole = "Manager",
                    TargetUserId = "All",
                    Title = "Occupancy Rate Update",
                    Message = "Current hotel occupancy is standing at 78%. Premium suites in high demand.",
                    Type = "info",
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new Notification
                {
                    NotificationId = 5,
                    TargetRole = "FrontDesk",
                    TargetUserId = "All",
                    Title = "Incoming Check-Ins Today",
                    Message = "5 confirmed guest arrivals scheduled for check-in afternoon session.",
                    Type = "reservation",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-45)
                },
                new Notification
                {
                    NotificationId = 6,
                    TargetRole = "Housekeeping",
                    TargetUserId = "All",
                    Title = "Clean Task Assignment",
                    Message = "Rooms 204 and 305 marked for turn-down service before 14:00.",
                    Type = "housekeeping",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15)
                }
            };
            await _context.Notifications.InsertManyAsync(initial);
        }
    }
}
