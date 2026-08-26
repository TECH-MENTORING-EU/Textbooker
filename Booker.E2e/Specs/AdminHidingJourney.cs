using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// Admin surface hiding: /Admin answers 404 to anonymous and regular users.
/// </summary>
[Collection("E2E")]
public class AdminHidingJourney(E2eWebAppFixture fixture)
{
    [Fact]
    public async Task Anonymous_gets_404()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);

        var response = await page.GotoAsync(fixture.BaseUrl + "/Admin/Index");

        Assert.Equal(404, response!.Status);
    }

    [Fact]
    public async Task Regular_user_gets_404()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");

        var response = await page.GotoAsync(fixture.BaseUrl + "/Admin/Index");

        Assert.Equal(404, response!.Status);
    }
}
