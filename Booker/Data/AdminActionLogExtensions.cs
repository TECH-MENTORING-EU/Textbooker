namespace Booker.Data;

public static class AdminActionLogExtensions
{
    // RODO - task 09: shared helper for writing an admin action log entry, to avoid
    // duplicating the same Add + SaveChangesAsync in every Admin page.
    public static async Task LogAdminActionAsync(
        this DataContext context,
        User? adminUser,
        string actionType,
        int targetId,
        string targetName,
        string targetType,
        string? parameters = null)
    {
        context.AdminActionLogs.Add(new AdminActionLog
        {
            AdminUserId = adminUser?.Id ?? 0,
            AdminUserName = adminUser?.UserName ?? adminUser?.Id.ToString() ?? "?",
            ActionType = actionType,
            TargetId = targetId,
            TargetName = targetName,
            TargetType = targetType,
            Parameters = parameters
        });
        await context.SaveChangesAsync();
    }
}
