using System;
using CogStay.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class Reservation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int ReservationId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    [BsonRepresentation(BsonType.String)]
    public ReservationStatus ReservationStatus { get; set; } = ReservationStatus.Pending;
    public int Version { get; set; } = 1;
}
