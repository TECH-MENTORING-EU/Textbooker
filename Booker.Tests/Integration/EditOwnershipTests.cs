using System.Net;
using Booker.Tests.Infrastructure;

namespace Booker.Tests.Integration;

/// <summary>
/// Edit page authorization (the a7cd534 class of bug - the delete handler once authorized
/// against the wrong resource): anonymous is redirected to login, a non-owner is forbidden,
/// the owner gets the form.
/// </summary>
public class EditOwnershipTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<(int Owner, int Other, int Item)> SeedAsync()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Edit school", "edit.edu.pl");
        var owner = await TestSeed.CreateUserAsync(factory.Services, "edit_owner", "edit_owner@edit.edu.pl", school);
        var other = await TestSeed.CreateUserAsync(factory.Services, "edit_other", "edit_other@edit.edu.pl", school);
        var item = await TestSeed.CreateItemAsync(factory.Services, owner);
        return (owner, other, item);
    }

    [Fact]
    public async Task Anonymous_edit_is_redirected_to_login()
    {
        var (_, _, item) = await SeedAsync();
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync($"/Edit/{item}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Non_owner_edit_is_forbidden()
    {
        var (_, _, item) = await SeedAsync();
        using var client = await factory.LoginAsync("edit_other@edit.edu.pl");

        var response = await client.GetAsync($"/Edit/{item}");

        // Forbid() under cookie authentication redirects to AccessDenied instead of
        // answering 403 directly - the resource is still denied.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Owner_gets_the_edit_form()
    {
        var (_, _, item) = await SeedAsync();
        using var client = await factory.LoginAsync("edit_owner@edit.edu.pl");

        var response = await client.GetAsync($"/Edit/{item}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_item_id_is_404_for_the_owner()
    {
        await SeedAsync();
        using var client = await factory.LoginAsync("edit_owner@edit.edu.pl");

        var response = await client.GetAsync("/Edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
