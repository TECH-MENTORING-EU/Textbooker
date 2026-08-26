using System.Net;
using Booker.Data;
using Booker.TestUtils;
using Booker.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Integration;

/// <summary>
/// Decimal price binding is culture-dependent on main: query strings bind invariantly
/// (12,50 silently becomes 1250 in the Browse price filters) while form fields bind the
/// request culture (pl-PL, so 12.50 is a binding error in Add/Edit). The fix lives on the
/// unmerged branch fix/k6-invariant-price-binding (commit e009560); these tests pin the
/// intended contract and stay skipped until that branch merges - un-skip them in its PR.
/// </summary>
public class PriceBindingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string SkipReason =
        "Red on main until fix/k6-invariant-price-binding (e009560) merges";

    [Fact(Skip = SkipReason)]
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

    [Fact(Skip = SkipReason)]
    public async Task Add_form_accepts_dot_decimal_price_under_plPL_culture()
    {
        var school = await TestSeed.CreateSchoolAsync(factory.Services, "Price school", "price.edu.pl");
        await TestSeed.CreateUserAsync(factory.Services, "price_owner2", "price_owner2@price.edu.pl", school);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var book = await context.Books
            .Include(b => b.Grades)
            .Where(b => b.Id > 0) // skip the Id=-1 "Inna" placeholder
            .OrderBy(b => b.Id)
            .FirstAsync();
        var grade = book.Grades.OrderBy(g => g.Id).First().GradeNumber;

        using var client = await factory.LoginAsync("price_owner2@price.edu.pl");
        var token = await client.GetAntiforgeryTokenAsync("/Add");

        var response = await client.PostAsync("/Add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Title"] = book.Title,
            ["Input.Subject"] = book.Subject.Name,
            ["Input.Grade"] = grade,
            ["Input.Level"] = book.Level.Name,
            ["Input.Description"] = "test",
            ["Input.State"] = "dobry",
            ["Input.Price"] = "12.50",
            ["__RequestVerificationToken"] = token,
        }));

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
