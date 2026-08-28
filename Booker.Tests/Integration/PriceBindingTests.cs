using System.Net;
using System.Net.Http.Headers;
using Booker.Data;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Integration;

/// <summary>
/// Decimal price binding used to be culture-dependent: query strings bound
/// invariantly (12,50 silently became 1250 in the Browse price filters) while
/// form fields bound the request culture (pl-PL, so 12.50 was a binding error
/// in Add/Edit). The invariant binder landed in main with e009560; these tests
/// pin the contract so it cannot regress.
/// </summary>
public class PriceBindingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Browse_query_price_filter_accepts_comma_decimals()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Price school", "price.edu.pl");
        var owner = await TestSeed.CreateUserAsync(factory.Services, "price_owner", "price_owner@price.edu.pl", school);
        var cheap = await TestSeed.CreateItemAsync(factory.Services, owner, price: 12.50m);
        var expensive = await TestSeed.CreateItemAsync(factory.Services, owner, price: 100m);

        var client = factory.CreateClient();

        // With the invariant binder "12,50" must filter at twelve and a half, not 1250:
        // the cheap item stays visible and the 100-zl one is filtered out. The gallery
        // tiles carry their item id in id="tile-title-{id}", so both sides of the
        // oracle assert on a marker the page really renders.
        var response = await client.GetAsync("/Browse?MinPrice=12%2C50&MaxPrice=12%2C50");

        Assert.True(response.IsSuccessStatusCode, $"got {(int)response.StatusCode}");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains($"tile-title-{cheap}", body);
        Assert.DoesNotContain($"tile-title-{expensive}", body);
    }

    [Fact]
    public async Task Add_form_accepts_dot_decimal_price_under_plPL_culture()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Price school", "price.edu.pl");
        await TestSeed.CreateUserAsync(factory.Services, "price_owner2", "price_owner2@price.edu.pl", school);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var book = await context.Books
            .Include(b => b.Grades)
            .Include(b => b.Subject)
            .Include(b => b.Level)
            .Where(b => b.Id > 0) // skip the Id=-1 "Inna" placeholder
            .OrderBy(b => b.Id)
            .FirstAsync();
        var grade = book.Grades.OrderBy(g => g.Id).First().GradeNumber;

        using var client = await factory.LoginAsync("price_owner2@price.edu.pl");
        var token = await client.GetAntiforgeryTokenAsync("/Add");

        // The Add form is multipart and requires at least one photo (the 400 the
        // form-urlencoded attempt produced is the server-side requireAtLeastOne gate).
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(book.Title), "Input.Title");
        content.Add(new StringContent(book.Subject!.Name), "Input.Subject");
        content.Add(new StringContent(book.Level!.Name), "Input.Level");
        content.Add(new StringContent(grade), "Input.Grade");
        content.Add(new StringContent("test"), "Input.Description");
        content.Add(new StringContent("dobry"), "Input.State");
        content.Add(new StringContent("12.50"), "Input.Price");
        content.Add(new StringContent(token), "__RequestVerificationToken");
        var image = new ByteArrayContent(TestImages.Jpeg);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(image, "Input.Images", "cover.jpg");

        var response = await client.PostAsync("/Add", content);

        Assert.True(response.IsSuccessStatusCode || (int)response.StatusCode == 302,
            $"got {(int)response.StatusCode}");

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<DataContext>();
        var created = await verifyContext.Items
            .Where(i => i.Description == "test")
            .OrderByDescending(i => i.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(created);
        Assert.Equal(12.50m, created.Price);
    }
}
