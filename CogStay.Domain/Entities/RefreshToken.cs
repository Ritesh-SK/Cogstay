using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string UserId { get; set; } = null!;
    public string UserType { get; set; } = null!; // Guest or Staff
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
