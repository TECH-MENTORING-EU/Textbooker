using System.Net;
using Booker.Tests.Infrastructure;

namespace Booker.Tests.Integration;

/// <summary>
/// The AdminHidden policy hides the admin area as 404 for anonymous visitors and
/// non-admins (anti-enumeration), while an admin gets the pages (StartupUtilities:
/// AuthorizeAreaFolder + the HideUnauthorized cookie-redirect overrides).
/// </summary>
public class AdminHidingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Anonymous_admin_request_is_404_not_a_redirect()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Admin/Index");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Non_admin_admin_request_is_404()
    {
        var userId = await TestSeed.CreateUserAsync(
            factory.Services, "admin_pleb", "admin_pleb@example.edu.pl", schoolId: null);
        using var client = await factory.LoginAsync("admin_pleb@example.edu.pl");

        var response = await client.GetAsync("/Admin/Index");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_gets_the_admin_index()
    {
        var userId = await TestSeed.CreateUserAsync(
            factory.Services, "admin_real", "admin_real@example.edu.pl", schoolId: null);
        await TestSeed.MakeAdminAsync(factory.Services, userId);
        using var client = await factory.LoginAsync("admin_real@example.edu.pl");

        var response = await client.GetAsync("/Admin/Index");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
