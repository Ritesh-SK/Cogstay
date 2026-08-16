using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class Feedback
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int FeedbackId { get; set; }
    public int GuestId { get; set; }
    public int? ReservationId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
