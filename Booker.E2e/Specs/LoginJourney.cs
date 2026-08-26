using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The real Identity login form: a wrong password yields the generic error
/// without leaking which field failed (s2 login-oracle fix), a correct login
/// greets the user in the header, and logout returns to the anonymous state.
/// </summary>
[Collection("E2E")]
public class LoginJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task Wrong_password_shows_the_generic_error_and_keeps_the_form()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl + "/Identity/Account/Login");

        await page.FillAsync("#Input_Email", "e2evictim@e2e.edu.pl");
        await page.FillAsync("input[type=password]", "Zle-Haslo1!");
        await page.ClickAsync("button[type=submit]");

        var summary = page.Locator("div.text-danger[role=alert]");
        await Assertions.Expect(summary).ToContainTextAsync("Błędny login lub hasło");
        Assert.Contains("/Login", page.Url);
        // The oracle contract: the failure summary never echoes the e-mail.
        var summaryText = await summary.TextContentAsync();
        Assert.DoesNotContain("e2evictim", summaryText);
    }

    [Fact]
    public async Task Correct_login_greets_the_user_in_the_header()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);

        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");

        await Assertions.Expect(page.GetByText("Witaj e2euser!")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Logout_returns_to_the_anonymous_header()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");

        await page.GetByText("Witaj e2euser!").ClickAsync(); // open the account dropdown
        await page.Locator("#logoutForm button").ClickAsync();
        await page.WaitForURLAsync(u => !u.Contains("/Logout"));

        await Assertions.Expect(page.Locator("#login")).ToContainTextAsync("Zaloguj się");
    }
}
