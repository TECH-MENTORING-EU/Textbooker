using System.Net;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Booker.Tests.Integration;

/// <summary>
/// The login page must answer every failure mode identically (audit finding S2, PR #70):
/// unknown email, wrong password, unconfirmed account and lockout outside Development all
/// render the same generic message with no distinguishing redirect or wording.
/// </summary>
public class LoginOracleTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private HttpClient NewFormClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        HandleCookies = true,
        AllowAutoRedirect = false,
    });

    [Fact]
    public async Task Login_succeeds_for_a_confirmed_seeded_user()
    {
        await TestSeed.CreateUserAsync(factory.Services, "oracle_ok", "oracle_ok@example.edu.pl", schoolId: null);

        using var client = NewFormClient();
        var response = await client.TryLoginAsync("oracle_ok@example.edu.pl", TestSeed.Password);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        // The issued cookie actually authenticates: the home page renders for the user.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);
    }

    [Fact]
    public async Task All_failure_modes_render_the_identical_generic_message()
    {
        await TestSeed.CreateUserAsync(
            factory.Services, "oracle_user", "oracle_user@example.edu.pl", schoolId: null);
        await TestSeed.CreateUserAsync(
            factory.Services, "oracle_unconfirmed", "oracle_unconfirmed@example.edu.pl", schoolId: null,
            configure: u => u.EmailConfirmed = false);
        await TestSeed.CreateUserAsync(
            factory.Services, "oracle_locked", "oracle_locked@example.edu.pl", schoolId: null);

        // Burn 5 failed attempts so the account is locked out for 5 minutes.
        var burnClient = NewFormClient();
        for (var i = 0; i < 5; i++)
        {
            await burnClient.TryLoginAsync("oracle_locked@example.edu.pl", "Zle-Haslo1!");
        }

        var scenarios = new (string Email, string Password)[]
        {
            ("nobody@example.edu.pl", TestSeed.Password),           // unknown email
            ("oracle_user@example.edu.pl", "Zle-Haslo1!"),          // wrong password
            ("oracle_unconfirmed@example.edu.pl", TestSeed.Password), // unconfirmed account
            ("oracle_locked@example.edu.pl", TestSeed.Password),    // locked-out account
        };

        var summaries = new List<string>();
        foreach (var (email, password) in scenarios)
        {
            var response = await NewFormClient().TryLoginAsync(email, password);

            // 200 with the form redisplayed - never a redirect that would leak account state.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            summaries.Add(await response.ExtractValidationSummaryAsync());
        }

        Assert.All(summaries, s => Assert.Equal(summaries[0], s));

        // The message must not echo the probed address nor disclose the lockout.
        Assert.DoesNotContain("example.edu.pl", summaries[0]);
        Assert.DoesNotContain("zablokowan", summaries[0], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("lock", summaries[0], StringComparison.OrdinalIgnoreCase);
    }
}
