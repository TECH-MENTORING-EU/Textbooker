using System.Net;
using System.Text.RegularExpressions;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Booker.Tests.Integration;

/// <summary>
/// The full account pipeline with the simulated mailer: the real Register form
/// creates an unconfirmed account and sends the activation e-mail (captured by
/// FakeEmailSender), the link taken from that e-mail confirms the account, and
/// only then does login succeed. Until then the login oracle (PR #70) keeps
/// answering with the generic message.
/// </summary>
public class RegistrationEmailConfirmationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly Regex LinkRegex = new("href='([^']+ConfirmEmail[^']+)'", RegexOptions.Compiled);

    private HttpClient NewFormClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        HandleCookies = true,
        AllowAutoRedirect = false,
    });

    [Fact]
    public async Task Register_confirmation_link_from_email_activates_login()
    {
        await TestSeed.CreateSchoolAsync(factory.Services, "Mail school", "mail.edu.pl");

        using var client = NewFormClient();
        var token = await client.GetAntiforgeryTokenAsync("/Identity/Account/Register");

        var response = await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.UserName"] = "mailuser",
            ["Input.Email"] = "mailuser@mail.edu.pl", // domain auto-assigns the school
            ["Input.Password"] = TestSeed.Password,
            ["Input.ConfirmPassword"] = TestSeed.Password,
            ["Input.AcceptTerms"] = "true",
            ["__RequestVerificationToken"] = token,
        }));

        // RequireConfirmedAccount: no sign-in, straight to the confirmation notice.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("RegisterConfirmation", response.Headers.Location!.ToString());

        // The simulated mailer captured exactly one welcome mail addressed to the user.
        var mail = Assert.Single(factory.Emails.Sent);
        Assert.Equal("mailuser@mail.edu.pl", mail.Email);
        Assert.Contains("TextBooker", mail.Subject);

        // Login is refused with the generic oracle message until the link is used.
        var early = await client.TryLoginAsync("mailuser@mail.edu.pl", TestSeed.Password);
        Assert.Equal(HttpStatusCode.OK, early.StatusCode); // 200 = form redisplayed, not signed in
        Assert.Equal("Błędny login lub hasło.", await early.ExtractValidationSummaryAsync());

        // Follow the real activation link from the e-mail body (HtmlEncode escaped the &'s).
        var link = LinkRegex.Match(mail.Body).Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(link), "activation link missing from welcome e-mail");
        var confirmed = await client.GetAsync(WebUtility.HtmlDecode(link));
        confirmed.EnsureSuccessStatusCode();
        var confirmedBody = await confirmed.Content.ReadAsStringAsync();
        Assert.Contains("aktywowane", confirmedBody);

        // Now the same credentials sign in and the header greets the new user.
        using var loggedIn = await factory.LoginAsync("mailuser@mail.edu.pl");
        var home = await loggedIn.GetAsync("/");
        var homeBody = await home.Content.ReadAsStringAsync();
        Assert.Contains("Witaj mailuser", homeBody);
    }
}
