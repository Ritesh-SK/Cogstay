using CogStay.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TaskStatus = CogStay.Domain.Enums.TaskStatus;

namespace CogStay.Domain.Entities;

public class HousekeepingTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public int TaskId { get; set; }
    public int RoomId { get; set; }
    public string TaskDescription { get; set; } = null!;
    [BsonRepresentation(BsonType.String)]
    public TaskStatus TaskStatus { get; set; } = TaskStatus.Pending;
}
