namespace Booker.Data;

public static class AdminActionLogExtensions
{
    // RODO — zadanie 09: wspólny zapis wpisu w dzienniku administracyjnym, żeby uniknąć
    // powielania tego samego Add + SaveChangesAsync w każdej stronie Admin.
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
