using CogStay.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CogStay.Domain.Entities;

public class Billing
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int BillId { get; set; }
    public int StayId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }
    [BsonRepresentation(BsonType.String)]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? Remarks { get; set; }
}
