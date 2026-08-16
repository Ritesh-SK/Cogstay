using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class StayRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int StayId { get; set; }
    public int GuestId { get; set; }
    public int ReservationId { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? BookingReference { get; set; }
    public string? BillingReference { get; set; }
    public string? StayDetails { get; set; }
}
