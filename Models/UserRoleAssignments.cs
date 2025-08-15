using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalManagementSystem.Models;

public class UserRoleAssignments
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserPermissionAssignments
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LoginActivityRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutTime { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
}
