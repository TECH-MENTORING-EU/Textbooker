using System.Net;
using Booker.Data;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Integration;

/// <summary>
/// The GDPR contract of admin account deletion (commit 6ea5f1a) over HTTP: the
/// R2 objects behind the profile picture and the items are purged together with
/// the account row, and only an admin can reach the handler.
/// </summary>
public class AccountDeletionTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<int> SeedVictimWithPhotosAsync()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Gdpr school", "gdpr.edu.pl");
        var victim = await TestSeed.CreateUserAsync(
            factory.Services, "gdpr_victim", "victim@gdpr.edu.pl", school,
            configure: u => u.Photo = "profile-key.png");
        await TestSeed.CreateItemAsync(factory.Services, victim, photo: "item-1.jpg;item-2.png");
        await TestSeed.CreateItemAsync(factory.Services, victim, photo: "https://cdn.test/external.png");
        factory.S3.Deletes.Clear();
        return victim;
    }

    private static async Task<HttpResponseMessage> PostDeleteAsync(
        HttpClient client, int id, string token) =>
        await client.PostAsync(
            "/Admin/Users?handler=Delete",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["id"] = id.ToString(),
                ["__RequestVerificationToken"] = token,
            }));

    [Fact]
    public async Task Admin_delete_purges_the_account_and_its_storage_objects()
    {
        var victim = await SeedVictimWithPhotosAsync();
        var adminSchool = await TestSeed.CreateSchoolAsync(factory.Services, "Admin school", "admin.edu.pl");
        var admin = await TestSeed.CreateUserAsync(factory.Services, "gdpr_admin", "gdpr_admin@admin.edu.pl", adminSchool);
        await TestSeed.MakeAdminAsync(factory.Services, admin);
        using var client = await factory.LoginAsync("gdpr_admin@admin.edu.pl");
        var token = await client.GetAntiforgeryTokenAsync("/");

        var response = await PostDeleteAsync(client, victim, token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("User deleted successfully.", await response.Content.ReadAsStringAsync());

        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            Assert.Null(await users.FindByIdAsync(victim.ToString()));
            Assert.NotNull(await users.FindByNameAsync("gdpr_admin"));
        }

        // Keys collected before the row cascade - exactly the bare storage keys.
        Assert.Equal(
            ["item-1.jpg", "item-2.png", "profile-key.png"],
            factory.S3.Deletes.Select(d => d.Key).ToArray());
    }

    [Fact]
    public async Task Unknown_user_id_is_404()
    {
        var adminSchool = await TestSeed.CreateSchoolAsync(factory.Services, "Admin school", "admin.edu.pl");
        var admin = await TestSeed.CreateUserAsync(factory.Services, "gdpr_admin", "gdpr_admin@admin.edu.pl", adminSchool);
        await TestSeed.MakeAdminAsync(factory.Services, admin);
        using var client = await factory.LoginAsync("gdpr_admin@admin.edu.pl");
        var token = await client.GetAntiforgeryTokenAsync("/");

        var response = await PostDeleteAsync(client, 999_999, token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Non_admin_cannot_delete_accounts()
    {
        var victim = await SeedVictimWithPhotosAsync();
        var bystanderSchool = await TestSeed.CreateSchoolAsync(factory.Services, "Bystander school", "bystander.edu.pl");
        await TestSeed.CreateUserAsync(factory.Services, "gdpr_bystander", "gdpr_bystander@bystander.edu.pl", bystanderSchool);
        using var client = await factory.LoginAsync("gdpr_bystander@bystander.edu.pl");
        var token = await client.GetAntiforgeryTokenAsync("/");

        var response = await PostDeleteAsync(client, victim, token);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        Assert.NotNull(await users.FindByIdAsync(victim.ToString()));
        Assert.Empty(factory.S3.Deletes);
    }
}
