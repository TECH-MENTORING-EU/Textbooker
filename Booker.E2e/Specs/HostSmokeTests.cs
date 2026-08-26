using System.Net;

using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>Temporary Step-2 verification: the Kestrel swap yields a reachable BaseUrl.</summary>
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
