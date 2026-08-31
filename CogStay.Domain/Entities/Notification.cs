using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int NotificationId { get; set; }
    public string TargetUserId { get; set; } = "All"; // GuestId string, StaffId string, or "All"
    public string TargetRole { get; set; } = "All";   // "Guest", "Admin", "FrontDesk", "Manager", "Housekeeping", or "All"
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "info";        // "info", "success", "warning", "reservation", "housekeeping", "system"
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
