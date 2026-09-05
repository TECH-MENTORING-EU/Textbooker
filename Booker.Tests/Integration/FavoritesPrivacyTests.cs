using System.Net;
using Booker.Data;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Integration;

/// <summary>
/// Favorites privacy (audit finding S1, PR #73): another user's favorites are served only
/// when the target is visible AND opted in; unknown, invisible and private targets all
/// answer with the same 404 so the response cannot be used to enumerate accounts.
/// </summary>
public class FavoritesPrivacyTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<(int Requester, int Private, int PublicOptIn, int Hidden, int PublicItemId)> SeedAsync()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Fav school", "fav.edu.pl");
        var requester = await TestSeed.CreateUserAsync(factory.Services, "fav_req", "fav_req@fav.edu.pl", school);
        var privateUser = await TestSeed.CreateUserAsync(
            factory.Services, "fav_priv", "fav_priv@fav.edu.pl", school); // AreFavoritesPublic defaults to false
        var publicUser = await TestSeed.CreateUserAsync(
            factory.Services, "fav_pub", "fav_pub@fav.edu.pl", school,
            configure: u => u.AreFavoritesPublic = true);
        var hiddenUser = await TestSeed.CreateUserAsync(
            factory.Services, "fav_hidden", "fav_hidden@fav.edu.pl", school,
            configure: u =>
            {
                u.AreFavoritesPublic = true;
                u.IsVisible = false;
            });
        var itemId = await TestSeed.CreateItemAsync(factory.Services, requester, price: 9.99m);

        // Give each target one favorite so a 200 would actually render content.
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Booker.Data.DataContext>();
        foreach (var userId in new[] { privateUser, publicUser, hiddenUser })
        {
            var user = await context.Users.FindAsync(userId);
            var item = await context.Items.FindAsync(itemId);
            user!.Favorites.Add(item!);
        }
        await context.SaveChangesAsync();

        return (requester, privateUser, publicUser, hiddenUser, itemId);
    }

    [Fact]
    public async Task Private_favorites_of_another_user_return_404()
    {
        var (_, privateUser, _, _, _) = await SeedAsync();
        using var client = await factory.LoginAsync("fav_req@fav.edu.pl");

        var response = await client.GetAsync($"/Profile/{privateUser}/Favorites");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Hidden_user_favorites_return_404_even_when_marked_public()
    {
        var (_, _, _, hiddenUser, _) = await SeedAsync();
        using var client = await factory.LoginAsync("fav_req@fav.edu.pl");

        var response = await client.GetAsync($"/Profile/{hiddenUser}/Favorites");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Public_opt_in_favorites_are_served_to_another_user()
    {
        var (_, _, publicUser, _, _) = await SeedAsync();
        using var client = await factory.LoginAsync("fav_req@fav.edu.pl");

        var response = await client.GetAsync($"/Profile/{publicUser}/Favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Own_favorites_render_without_an_id_parameter()
    {
        var (requester, _, _, _, _) = await SeedAsync();
        using var client = await factory.LoginAsync("fav_req@fav.edu.pl");

        var byNone = await client.GetAsync("/Profile/Favorites");
        var byOwnId = await client.GetAsync($"/Profile/{requester}/Favorites");

        Assert.Equal(HttpStatusCode.OK, byNone.StatusCode);
        Assert.Equal(HttpStatusCode.OK, byOwnId.StatusCode);
    }

    [Fact]
    public async Task Anonymous_own_favorites_redirect_to_login()
    {
        var client = factory.CreateNoRedirectClient();

        var response = await client.GetAsync("/Profile/Favorites");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Anonymous_favorites_with_an_id_are_served_when_public()
    {
        // The spec's conditional branch, resolved against the real handler: favorites
        // with an explicit id skip the login redirect, so an opted-in target is
        // readable anonymously (NOTES: the no-id form redirects, see the test above).
        var (_, _, publicUser, _, _) = await SeedAsync();
        var client = factory.CreateNoRedirectClient();

        var response = await client.GetAsync($"/Profile/{publicUser}/Favorites");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_user_favorites_return_404()
    {
        await SeedAsync();
        using var client = await factory.LoginAsync("fav_req@fav.edu.pl");

        var response = await client.GetAsync("/Profile/999999/Favorites");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
