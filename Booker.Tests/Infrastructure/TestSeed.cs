using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Infrastructure;

/// <summary>
/// Deterministic seed data for the Testing environment (the app's own seeding runs only
/// in Development and is randomized). Mirrors SeedData.EnsureCredentialUsersAsync.
/// </summary>
public static class TestSeed
{
    // Same constant the development seed uses (Booker/Data/SeedData.cs).
    public const string Password = "TestPass123!";

    public static async Task<int> CreateSchoolAsync(IServiceProvider services, string name, string domain)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var existing = await context.Schools.SingleOrDefaultAsync(s => s.EmailDomain == domain);
        if (existing != null)
        {
            return existing.Id;
        }
        var school = new School { Name = name, EmailDomain = domain, IsActive = true };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        return school.Id;
    }

    /// <summary>
    /// Creates a confirmed, login-able user (idempotent: returns the existing id when the
    /// name is already taken - one factory serves the whole test class, so per-test seeds
    /// re-run with the same names). Privacy flags and other property tweaks go through
    /// <paramref name="configure"/> (runs before CreateAsync persists the user).
    /// </summary>
    public static async Task<int> CreateUserAsync(
        IServiceProvider services,
        string userName,
        string email,
        int? schoolId,
        Action<User>? configure = null,
        string password = Password)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var existing = await userManager.FindByNameAsync(userName);
        if (existing != null)
        {
            return existing.Id;
        }
        var user = new User
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true, // RequireConfirmedAccount = true in Program.cs
            SchoolId = schoolId,
        };
        configure?.Invoke(user);

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Seed user creation failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        return user.Id;
    }

    public static async Task<int> CreateItemAsync(
        IServiceProvider services,
        int ownerId,
        decimal price = 20m,
        bool isVisible = true,
        string photo = "",
        string description = "seed item",
        string state = "dobry")
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var book = await context.Books.OrderBy(b => b.Id).FirstAsync();
        var owner = await context.Users.FindAsync(ownerId)
            ?? throw new InvalidOperationException($"seed user {ownerId} missing");

        var item = new Item
        {
            BookId = book.Id,
            UserId = ownerId,
            Book = book,
            User = owner,
            Description = description,
            State = state,
            Price = price,
            IsVisible = isVisible,
            CreatedAt = DateTime.Now,
            Photo = photo,
        };
        context.Items.Add(item);
        await context.SaveChangesAsync();
        return item.Id;
    }

    public static async Task<User> GetUserAsync(IServiceProvider services, int userId)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"user {userId} missing");
    }

    public static async Task MakeAdminAsync(IServiceProvider services, int userId)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"user {userId} missing");
        // The Admin role is created by InitializeRolesAsync on every boot, including Testing.
        await userManager.AddToRoleAsync(user, "Admin");
    }
}
