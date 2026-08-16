using System;
using CogStay.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class OtpRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string UserId { get; set; } = null!;
    public string Target { get; set; } = null!;
    [BsonRepresentation(BsonType.String)]
    public OtpType OtpType { get; set; }
    public string CodeHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTime LastSentAt { get; set; } = DateTime.UtcNow;
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
