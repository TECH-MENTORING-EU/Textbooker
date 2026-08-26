using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The buyer's detail-page journey: the HTMX contact reveal swaps in the
/// seller's e-mail, the favorites button toggles without a reload, and the
/// favorites page lists what was added.
/// </summary>
[Collection("E2E")]
public class BookDetailJourney(E2eWebAppFixture fixture)
{
    private async Task<IPage> LoggedInBuyerOnItemPageAsync()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");
        await page.GotoAsync($"{fixture.BaseUrl}/Book/{fixture.SeededItemId}");
        return page;
    }

    [Fact]
    public async Task Anonymous_contact_reveal_redirects_to_login()
    {
        var page = await Browsers.Shared.NewPageAsync($"{fixture.BaseUrl}/Book/{fixture.SeededItemId}");

        await page.GetByRole(AriaRole.Button, new() { Name = "Zapytaj o przedmiot" }).ClickAsync();

        await page.WaitForURLAsync(u => u.Contains("/Identity/Account/Login"));
    }

    [Fact]
    public async Task Contact_reveal_swaps_in_the_sellers_email()
    {
        var page = await LoggedInBuyerOnItemPageAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Zapytaj o przedmiot" }).ClickAsync();

        await Assertions.Expect(page.GetByText("e2eother@e2e.edu.pl")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Favorites_button_toggles_and_the_item_lands_on_the_favorites_page()
    {
        var page = await LoggedInBuyerOnItemPageAsync();
        var title = (await page.TextContentAsync("h1#book-title"))!.Trim();
        var add = page.GetByRole(AriaRole.Button, new() { Name = "Dodaj do ulubionych" });

        await add.ClickAsync();
        await Assertions
            .Expect(page.GetByRole(AriaRole.Button, new() { Name = "Usuń z ulubionych" }))
            .ToBeVisibleAsync();

        try
        {
            await page.GotoAsync(fixture.BaseUrl + "/Profile/Favorites");
            var body = await page.TextContentAsync("body");
            Assert.Contains(title, body); // the seeded item is listed by its title...
            Assert.Contains("12,50", body); // ...with its price
        }
        finally
        {
            // Toggle back on the item page so the shared fixture stays clean even
            // when an assertion above failed.
            await page.GotoAsync($"{fixture.BaseUrl}/Book/{fixture.SeededItemId}");
            var remove = page.GetByRole(AriaRole.Button, new() { Name = "Usuń z ulubionych" }).First;
            if (await remove.IsVisibleAsync())
            {
                await remove.ClickAsync();
            }
        }
    }
}
