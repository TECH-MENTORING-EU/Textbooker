using System.Net;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;

namespace Booker.Tests.Integration;

/// <summary>
/// Cross-school isolation on the book detail page (commit 680a224): OnGet, the HTMX
/// contact-reveal handler (OnGetEmail) and the reserve handler (OnPostReserve) must all
/// resolve the item through the school-aware overload, so a user from another school gets
/// a plain 404 everywhere instead of the seller's data.
/// </summary>
public class BookPageIsolationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private sealed record IsoSeed(int ItemB, int ItemC, int ItemCHidden);

    private async Task<IsoSeed> SeedAsync()
    {
        var schoolA = await TestSeed.CreateSchoolAsync(factory.Services, "Szkoła A", "a.edu.pl");
        var schoolB = await TestSeed.CreateSchoolAsync(factory.Services, "Szkoła B", "b.edu.pl");
        await TestSeed.CreateUserAsync(factory.Services, "iso_a", "iso_a@a.edu.pl", schoolA);
        await TestSeed.CreateUserAsync(factory.Services, "iso_d", "iso_d@b.edu.pl", schoolB); // same-school viewer
        var userB = await TestSeed.CreateUserAsync(factory.Services, "iso_b", "iso_b@b.edu.pl", schoolB);
        var userC = await TestSeed.CreateUserAsync(
            factory.Services, "iso_c", "iso_c@b.edu.pl", schoolB,
            configure: u =>
            {
                u.DisplayPhone = false;
                u.PhoneNumber = "600100200";
            });
        var itemB = await TestSeed.CreateItemAsync(factory.Services, userB, price: 20m);
        var itemC = await TestSeed.CreateItemAsync(factory.Services, userC, price: 25m);
        var itemCHidden = await TestSeed.CreateItemAsync(factory.Services, userC, isVisible: false);
        return new IsoSeed(itemB, itemC, itemCHidden);
    }

    [Fact]
    public async Task Anonymous_visitor_sees_items_from_all_schools()
    {
        var seed = await SeedAsync();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/Book/{seed.ItemB}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task User_from_another_school_gets_404_on_all_three_handlers()
    {
        var seed = await SeedAsync();
        using var client = await factory.LoginAsync("iso_a@a.edu.pl");

        var page = await client.GetAsync($"/Book/{seed.ItemB}");
        Assert.Equal(HttpStatusCode.NotFound, page.StatusCode);

        client.DefaultRequestHeaders.Add("HX-Request", "true");
        var email = await client.GetAsync($"/Book/{seed.ItemB}?handler=Email");
        Assert.Equal(HttpStatusCode.NotFound, email.StatusCode);

        var token = await client.GetAntiforgeryTokenAsync("/");
        var reserve = await client.PostAsync(
            $"/Book/{seed.ItemB}?handler=Reserve&reserve=true",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["reserve"] = "true",
            }));
        Assert.Equal(HttpStatusCode.NotFound, reserve.StatusCode);
    }

    [Fact]
    public async Task Same_school_user_gets_the_contact_partial_with_the_seller_email()
    {
        var seed = await SeedAsync();
        using var client = await factory.LoginAsync("iso_d@b.edu.pl");
        client.DefaultRequestHeaders.Add("HX-Request", "true");

        var response = await client.GetAsync($"/Book/{seed.ItemB}?handler=Email");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("iso_b@b.edu.pl", body);
        Assert.Contains("mailto:iso_b@b.edu.pl", body);
    }

    [Fact]
    public async Task Contact_partial_hides_the_phone_when_DisplayPhone_is_false()
    {
        // Audit finding S5 / PR #72: the partial must gate the phone on DisplayPhone.
        var seed = await SeedAsync();
        using var client = await factory.LoginAsync("iso_d@b.edu.pl");
        client.DefaultRequestHeaders.Add("HX-Request", "true");

        var response = await client.GetAsync($"/Book/{seed.ItemC}?handler=Email");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("iso_c@b.edu.pl", body); // email still visible, only the phone is gated
        Assert.DoesNotContain("600100200", body);
    }

    [Fact]
    public async Task Anonymous_contact_reveal_returns_204_with_HX_Redirect_to_login()
    {
        var seed = await SeedAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("HX-Request", "true");

        var response = await client.GetAsync($"/Book/{seed.ItemC}?handler=Email");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var redirect = Assert.Single(response.Headers.GetValues("HX-Redirect"));
        Assert.Contains("/Identity/Account/Login", redirect);
    }

    [Fact]
    public async Task Hidden_item_is_404_for_a_same_school_non_owner()
    {
        var seed = await SeedAsync();
        using var client = await factory.LoginAsync("iso_b@b.edu.pl"); // school B, not the owner

        var response = await client.GetAsync($"/Book/{seed.ItemCHidden}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
