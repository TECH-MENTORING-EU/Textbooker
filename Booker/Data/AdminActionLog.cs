namespace Booker.Data;

// RODO - task 09: administrative action log. Deliberately without foreign keys/navigation to
// User - an entry must survive deletion of the account or other object it refers to.
public class AdminActionLog
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public required string AdminUserName { get; set; }
    public required string ActionType { get; set; }
    public int TargetId { get; set; }
    public required string TargetName { get; set; }
    public required string TargetType { get; set; }
    public string? Parameters { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public static class AdminActionTypes
{
    public const string UserLockout = "UserLockout";
    public const string UserUnlock = "UserUnlock";
    public const string UserDelete = "UserDelete";
    public const string AdminRoleGranted = "AdminRoleGranted";
    public const string AdminRoleRemoved = "AdminRoleRemoved";
    public const string SchoolCreated = "SchoolCreated";
    public const string SchoolDeactivated = "SchoolDeactivated";
    public const string SchoolReactivated = "SchoolReactivated";
    public const string SchoolUpdated = "SchoolUpdated";
}
