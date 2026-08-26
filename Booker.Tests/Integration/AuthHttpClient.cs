using System.Net;
using System.Text.RegularExpressions;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Booker.Tests.Integration;

public static class AuthHttpClient
{
    private static readonly Regex TokenRegex =
        new("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.Compiled);

    /// <summary>Fetches a page and extracts an antiforgery token for a later POST.</summary>
    public static async Task<string> GetAntiforgeryTokenAsync(this HttpClient client, string path)
    {
        var body = await client.GetStringAsync(path);
        var match = TokenRegex.Match(body);
        return match.Success
            ? match.Groups[1].Value
            : throw new InvalidOperationException($"no antiforgery token found on {path}");
    }

    /// <summary>Creates an authenticated client by posting the real login form.</summary>
    public static async Task<HttpClient> LoginAsync(
        this CustomWebApplicationFactory factory, string email, string password = TestSeed.Password)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false,
        });

        var response = await client.TryLoginAsync(email, password);
        if ((int)response.StatusCode != 302)
        {
            throw new InvalidOperationException(
                $"login as {email} failed: {(int)response.StatusCode} {response.StatusCode}");
        }
        return client;
    }

    /// <summary>Posts the login form and returns the raw response (no 302 assertion).</summary>
    public static async Task<HttpResponseMessage> TryLoginAsync(
        this HttpClient client, string email, string password)
    {
        var token = await client.GetAntiforgeryTokenAsync("/Identity/Account/Login");
        return await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        }));
    }

    /// <summary>Extracts the model-level validation summary text (the ul inside the text-danger alert).</summary>
    public static async Task<string> ExtractValidationSummaryAsync(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            body,
            "<div[^>]*class=\"[^\"]*text-danger[^\"]*\"[^>]*>\\s*<ul>\\s*<li>(.*?)</li>",
            RegexOptions.Singleline);
        Assert.True(match.Success, "validation summary not found in response");
        return WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
    }
}
