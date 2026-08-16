using CogStay.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class Room
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string RoomType { get; set; } = null!;
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PricePerNight { get; set; }
    [BsonRepresentation(BsonType.String)]
    public RoomStatus Status { get; set; } = RoomStatus.Available;
}
