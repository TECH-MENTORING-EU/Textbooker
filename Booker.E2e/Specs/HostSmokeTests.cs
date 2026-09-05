using System.Net;

using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// Guards the fixture itself: the Kestrel swap must yield a BaseUrl that a plain
/// HttpClient and a real Chromium can reach. If this fails, every journey will
/// too - better to fail here first with an obvious name.
/// </summary>
[Collection("E2E")]
public class HostSmokeTests(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task The_real_server_answers_on_localhost()
    {
        Assert.StartsWith("http://127.0.0.1:", fixture.BaseUrl);

        using var http = new HttpClient();
        var response = await http.GetAsync(fixture.BaseUrl + "/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Chromium_loads_the_home_page()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);

        Assert.NotEmpty(await page.TitleAsync());
    }
}
