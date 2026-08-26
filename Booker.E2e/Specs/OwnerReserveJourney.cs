using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The owner's reserve toggle on the detail page: the checkbox posts via HTMX
/// and HX-Refresh reloads the page with the reserved banner.
/// </summary>
[Collection("E2E")]
public class OwnerReserveJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task Checking_reserve_marks_the_item_reserved_after_refresh()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2eother@e2e.edu.pl"); // owner of the seeded item
        await page.GotoAsync($"{fixture.BaseUrl}/Book/{fixture.SeededItemId}");

        await page.Locator("#reserve-cb").CheckAsync();

        try
        {
            await Assertions
                .Expect(page.GetByRole(AriaRole.Status))
                .ToContainTextAsync("zarezerwowany");
            Assert.True(await page.Locator("#reserve-cb").IsCheckedAsync());
        }
        finally
        {
            // Restore the fixture state for any test that runs after this one,
            // even when an assertion above failed.
            if (await page.Locator("#reserve-cb").IsCheckedAsync())
            {
                await page.Locator("#reserve-cb").UncheckAsync();
            }
        }
    }
}
