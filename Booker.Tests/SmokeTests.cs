using System.Net;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests;

public class SmokeTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Home_page_returns_html()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Browse_page_returns_html()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Browse");

        Assert.True(response.IsSuccessStatusCode, $"got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Unknown_book_id_returns_404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Book/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_area_returns_404_for_anonymous_visitor()
    {
        // The AdminHidden policy hides the folder as 404, not 403/302
        // (StartupUtilities: AuthorizeAreaFolder + ConfigureAuthorization redirect overrides).
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Admin/Index");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_user_profile_returns_200()
    {
        var userId = await TestSeed.CreateUserAsync(
            factory.Services, "seedu1", "seedu1@example.edu.pl", schoolId: null);

        var client = factory.CreateClient();

        var response = await client.GetAsync($"/Profile/{userId}");

        Assert.True(response.IsSuccessStatusCode, $"got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Seeded_items_are_queryable_from_the_test_host()
    {
        var ownerId = await TestSeed.CreateUserAsync(
            factory.Services, "seedu2", "seedu2@example.edu.pl", schoolId: null);
        var itemId = await TestSeed.CreateItemAsync(factory.Services, ownerId, price: 12.50m);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Booker.Data.DataContext>();
        var item = await context.Items.FindAsync(itemId);

        Assert.NotNull(item);
        Assert.Equal(12.50m, item.Price);
    }
}
