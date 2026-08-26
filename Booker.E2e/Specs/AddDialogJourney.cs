using System.Text;

using Microsoft.Playwright;

namespace Booker.E2e.Specs;

/// <summary>
/// The add-listing dialog (5d0f5b0 + 3bb4db9): closing the summary modal must
/// not submit anything, and double-clicking the confirm button must produce
/// exactly one POST. The upload travels through the canvas re-encode in
/// site.js, so a real JPEG payload is required.
/// </summary>
[Collection("E2E")]
public class AddDialogJourney(E2eWebAppFixture fixture)
{
    // Minimal valid 1x1 white JPEG for the canvas pipeline.
    private static readonly byte[] JpegBytes = Convert.FromBase64String(
        "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAABAAEDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD3+iiigD//2Q==");

    private static List<string> TrackPosts(IPage page)
    {
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(request.Url);
            }
        };
        return posts;
    }

    private static async Task FillValidFormAsync(IPage page)
    {
        // Order matters: each pick narrows the dependent selects to values
        // compatible with the chosen book.
        await page.SelectAndSettleAsync("Input_Title", 1);
        await page.SelectAndSettleAsync("Input_Subject", 1);
        await page.SelectAndSettleAsync("Input_Grade", 1);
        await page.SelectAndSettleAsync("Input_Level", 1);

        await page.FillAsync("#Input_Description", "spotkanie przy szkole");
        await page.FillAsync("#Input_State", "jak nowa");
        await page.FillAsync("#Input_Price", "15"); // integer avoids the pl-PL comma binding gap (k6)
    }

    [Fact]
    public async Task Closing_the_summary_modal_sends_zero_posts()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");
        await page.GotoAsync(fixture.BaseUrl + "/Add");
        await FillValidFormAsync(page);
        // Client-side validation refuses to open the dialog without a photo
        // (mirrors the server's requireAtLeastOne), so upload one first.
        await page.SetInputFilesAsync("#add-image", new FilePayload
        {
            Name = "cover.jpg",
            MimeType = "image/jpeg",
            Buffer = JpegBytes,
        });
        await Assertions.Expect(page.Locator(".image-preview-container img").First).ToBeVisibleAsync();
        var posts = TrackPosts(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Dalej" }).ClickAsync();
        await Assertions.Expect(page.Locator("#summaryModal")).ToBeVisibleAsync();
        await page.Locator("#summaryModal button[aria-label='Close']").ClickAsync();
        await Assertions.Expect(page.Locator("#summaryModal")).Not.ToBeVisibleAsync();

        Assert.False(posts.Any(u => u.EndsWith("/Add")), "closing the modal must not POST");
    }

    [Fact]
    public async Task Double_clicking_confirm_submits_exactly_once()
    {
        var page = await Browsers.Shared.NewPageAsync(fixture.BaseUrl);
        await page.LoginAsync(fixture.BaseUrl, "e2euser@e2e.edu.pl");
        await page.GotoAsync(fixture.BaseUrl + "/Add");
        await FillValidFormAsync(page);

        await page.SetInputFilesAsync("#add-image", new FilePayload
        {
            Name = "cover.jpg",
            MimeType = "image/jpeg",
            Buffer = JpegBytes,
        });
        // Canvas re-encode runs async; it toggles aria-busy on the file input and
        // only then renders the preview thumbnails.
        await Assertions.Expect(page.Locator("#add-image")).ToHaveAttributeAsync("aria-busy", "false");
        await Assertions.Expect(page.Locator(".image-preview-container img").First).ToBeVisibleAsync();
        var putsBefore = fixture.S3.Puts.Count;

        await page.GetByRole(AriaRole.Button, new() { Name = "Dalej" }).ClickAsync();
        await Assertions.Expect(page.Locator("#summaryModal")).ToBeVisibleAsync();
        var posts = TrackPosts(page);

        await page.Locator("#confirmAddBtn").DblClickAsync();
        await page.WaitForURLAsync(u => new Uri(u).AbsolutePath.StartsWith("/Book/"));

        var addPosts = posts.Where(u => new Uri(u).AbsolutePath == "/Add").ToList();
        Assert.Single(addPosts);
        Assert.Equal(putsBefore + 1, fixture.S3.Puts.Count); // one photo stored
        var body = await page.TextContentAsync("body");
        Assert.Contains("15,00", body); // the new listing's price renders on its page
    }
}
