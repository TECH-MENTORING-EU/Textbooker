using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The landing page entry points (b0e54e4 filter-loop territory): category
/// chips must navigate to /Browse with a subject filter, and the hero CTA to
/// /Browse plain.
/// </summary>
[Collection("E2E")]
public class HomeJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task Category_chip_navigates_to_browse_with_the_subject_filter()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);

        var chip = page.Locator(".category-badges .category-tile a").First;
        var subject = await chip.GetAttributeAsync("href");
        Assert.NotNull(subject);
        Assert.Contains("/Browse", subject);
        Assert.Contains("subject=", subject);

        await chip.ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Browse"));

        Assert.Contains(subject, page.Url); // landed on exactly the chip's target
        Assert.NotNull(await page.QuerySelectorAsync(".grid-gallery"));
    }

    [Fact]
    public async Task Hero_browse_button_opens_the_gallery()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);

        await page.GetByRole(AriaRole.Button, new() { Name = "Przeglądaj oferty" }).ClickAsync();
        await page.WaitForURLAsync(u => u.Contains("/Browse"));

        Assert.EndsWith("/Browse", page.Url.Split('?')[0]);
    }
}
