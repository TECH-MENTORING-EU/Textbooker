using Booker.TestUtils;
using Microsoft.Playwright;

namespace Booker.E2e;

/// <summary>
/// One Chromium for the whole (serialized) E2E assembly; a fresh context per
/// spec keeps cookies isolated. Seeded demo photos point at images.unsplash.com
/// - blocked at the network layer so pages never wait on a real CDN.
/// </summary>
public sealed class PlaywrightBrowser : IAsyncDisposable
{
    private readonly IPlaywright _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
    private readonly IBrowser _browser;

    public PlaywrightBrowser() =>
        _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
            .GetAwaiter().GetResult();

    public Task<IBrowserContext> NewContextAsync() =>
        _browser.NewContextAsync(new BrowserNewContextOptions { Locale = "pl-PL" });

    public async ValueTask DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}

/// <summary>xUnit collections run serially (xunit.runner.json); one browser, many contexts.</summary>
public static class Browsers
{
    public static readonly PlaywrightBrowser Shared = new();
}

public static class PlaywrightBrowserExtensions
{
    public static async Task<IPage> NewPageAsync(this PlaywrightBrowser browser, string baseUrl)
    {
        var context = await browser.NewContextAsync();
        await context.RouteAsync("**://images.unsplash.com/**", route => route.AbortAsync());
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(15_000);
        await page.GotoAsync(baseUrl);
        return page;
    }

    /// <summary>Logs in through the real Identity form and waits for the header switch.</summary>
    public static async Task LoginAsync(this IPage page, string baseUrl, string email)
    {
        await page.GotoAsync(baseUrl + "/Identity/Account/Login");
        // The login page labels its e-mail field as plain text ("Nazwa użytkownika / e-mail").
        await page.FillAsync("#Input_Email", email);
        await page.FillAsync("input[type=password]", TestSeed.Password);
        await page.ClickAsync("button[type=submit]");
        await page.WaitForURLAsync(u => !u.Contains("/Login"));
        // The header swap is the real completion signal: the authenticated layout
        // carries #logoutForm, the anonymous one does not. Attached, not Visible -
        // the form sits inside the closed account dropdown.
        await page.WaitForSelectorAsync("#logoutForm", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Attached,
        });
    }

    /// <summary>
    /// Selects the nth non-empty option and waits out the Params cascade: every
    /// change fires an HTMX oob-swap of all four selects whose response can land
    /// after the network goes idle, resetting dependent fields to the request's
    /// echoed values - so if the chosen value got wiped by a late swap, select
    /// it again on the settled DOM.
    /// </summary>
    public static async Task SelectAndSettleAsync(this IPage page, string selectId, int optionIndex)
    {
        var select = page.Locator($"#{selectId}");
        var value = await select.Locator("option:not([value=''])").Nth(optionIndex - 1)
            .GetAttributeAsync("value")
            ?? throw new InvalidOperationException($"#{selectId} has no option #{optionIndex}");
        await select.SelectOptionAsync(value);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (await page.Locator($"#{selectId}").InputValueAsync() != value)
        {
            await page.Locator($"#{selectId}").SelectOptionAsync(value);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        await Assertions.Expect(page.Locator($"#{selectId}")).ToHaveValueAsync(value);
    }
}
