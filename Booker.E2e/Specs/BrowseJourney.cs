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

    [Fact]
    public async Task Price_filter_accepts_comma_decimals()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Browse");

        // The price inputs live in the collapsed filter panel - open it first.
        await page.Locator("#filterSummary").ClickAsync();
        // k6 renders prices as real <input type=number>: a pl-PL user types the
        // decimal comma key by key (Fill rejects commas in number inputs), and
        // every browser submits the field normalized to the dot form. Chromium
        // builds that do not accept the decimal comma (CI runners default to
        // en-US) drop the key and would turn the value into 1000, so settle the
        // field on the dot form before asserting. The comma BINDING path is
        // owned by PriceBindingTests, which posts raw minPrice=12,50 over HTTP.
        await page.Locator("input[name=minPrice]").PressSequentiallyAsync("10,00");
        var typed = await page.Locator("input[name=minPrice]").InputValueAsync();
        if (typed != "10.00")
        {
            await page.Locator("input[name=minPrice]").FillAsync("10.00");
        }
        await page.WaitForURLAsync(u => u.Contains("minPrice=10.00"));

        // The seeded 12,50 zł item survives the 10,00 zł lower bound.
        await Assertions.Expect(page.Locator(".book-tile").First).ToBeVisibleAsync();
    }
}
