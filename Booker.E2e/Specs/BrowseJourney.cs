using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// HTMX-driven filtering on /Browse (the browse.js chips + debounced search):
/// the gallery swaps in place and the URL tracks the filter state.
/// </summary>
[Collection("E2E")]
public class BrowseJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task The_gallery_renders_seeded_items_with_pl_prices()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        await Assertions.Expect(page.Locator(".grid-gallery .book-tile").First).ToBeVisibleAsync();
        // Culture is forced pl-PL: 12.50m renders with a comma decimal. Journeys
        // may add newer listings, so do not assume the seeded one is first.
        await Assertions
            .Expect(page.Locator(".book-tile p").Filter(new() { HasText = "12,50" }).First)
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Changing_the_grade_filter_updates_the_url_and_the_gallery()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        // The filters live in a collapsed <details>; open it to reach the select.
        await page.Locator("#filterSummary").ClickAsync();
        var gradeSelect = page.Locator("select[name=grade]");
        var option = await gradeSelect.Locator("option:not([value=''])").First
            .GetAttributeAsync("value")
            ?? throw new InvalidOperationException("grade filter has no options");
        await gradeSelect.SelectOptionAsync(option);

        await page.WaitForURLAsync(u => u.Contains("grade="));
        Assert.Contains($"grade={Uri.EscapeDataString(option)}", page.Url);
        // The swap target stays mounted (htmx replaced only .grid-gallery content).
        await Assertions.Expect(page.Locator(".grid-gallery")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Search_input_triggers_the_debounced_update()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        await page.FillAsync("input[type=search]", "xyz-no-such-book");
        await page.WaitForURLAsync(u => u.Contains("search="));

        Assert.Contains("search=xyz-no-such-book", page.Url);
    }

    [Fact(Skip = "Red on main until fix/k6-invariant-price-binding (e009560) merges")]
    public async Task Price_filter_accepts_comma_decimals()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        await page.FillAsync("input[name=minPrice]", "10,00");
        await page.WaitForURLAsync(u => u.Contains("minPrice="));

        // The seeded 12,50 zł item survives the 10,00 zł lower bound.
        await Assertions.Expect(page.Locator(".book-tile").First).ToBeVisibleAsync();
    }
}
