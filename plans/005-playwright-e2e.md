# Plan 005: Automated frontend E2E suite (Playwright over the real app, in-process)

> **Executor instructions**: Follow this plan step by step. Run every
> verification command and confirm the expected result before moving to the
> next step. If anything in the "STOP conditions" section occurs, stop and
> report — do not improvise. When done, update the status row for this plan
> in `plans/README.md` — unless a reviewer dispatched you and told you they
> maintain the index.
>
> **Drift check (run first)**: `git diff --stat 32efc8d..HEAD -- Booker/wwwroot/js/site.js Booker/wwwroot/js/browse.js Booker/wwwroot/js/add-page.js Booker/Pages/Add.cshtml Booker/Pages/Browse.cshtml Booker/Pages/Index.cshtml`
> If any in-scope file changed since this plan was written, compare the
> "Current state" excerpts against the live code before proceeding; on a
> mismatch, treat it as a STOP condition.

## Status

- **Priority**: P2
- **Effort**: L
- **Risk**: MED (new tooling + Kestrel-host boot pattern; no production code changes)
- **Depends on**: plans/001-test-foundation.md
- **Category**: tests
- **Planned at**: commit `32efc8d`, 2026-08-26

## Why this matters

Six of the ~19 audited regression fixes lived entirely in browser behavior
the server cannot see: the add-listing dialog's close button implicitly
submitting the form (`5d0f5b0`), double submissions creating duplicate
listings (`3bb4db9`), duplicate validation-script initialization
(`243fb7e`), the filter loop on the home page (`b0e54e4`), fragile grade
parsing (`938be4f`), and the book-tile cover photo not being a link
(`769ef06`). These flows — favorites toggle, contact reveal, reserve,
HTMX-driven filter updates, the image upload dialog — are exercised only by
a real browser. This plan adds a Playwright (Chromium) E2E suite that boots
the actual app **in-process** with the same fake adapters as the integration
tests (SQLite in-memory, fake S3, fake email), so there is no docker, no real
SMTP/R2, and no environment drift between CI and local runs.

## Current state

- The app's interactive surface (`32efc8d`):
  - Browse form (`Booker/Pages/Browse.cshtml:9-12`):
    `hx-get="/Browse" hx-trigger="submit, input changed delay:500ms, change"
    hx-target=".grid-gallery" hx-push-url`; chips set hidden inputs and call
    `requestSubmit` (`Booker/wwwroot/js/browse.js:2-23`); the server returns
    the `ItemGallery` ViewComponent partial only when the `HX-Request`
    header is present (`Booker/Pages/Browse.cshtml.cs:73-81`).
  - Favorites buttons (`Booker/Pages/Shared/_FavoriteButton.cshtml:5-35`):
    `hx-post` to `/Profile/Favorites` handlers `Add`/`Remove`,
    `hx-swap="outerHTML"`; anonymous gets `403` + `HX-Redirect` to login.
  - Book page: `hx-get="?handler=Email"` reveals `_ContactDetails`;
    reserve checkbox `hx-post="?handler=Reserve"` (owner-only).
  - Add page (`Booker/Pages/Add.cshtml:12,76-94` + `site.js`): selected
    images are re-encoded on canvas (max 800×600 JPEG) and swapped into the
    input's `FileList`; the upload UI toggles `aria-busy` on the form while
    processing; submit opens `#summaryModal`; the real POST happens only
    from `#confirmAddBtn`, and `add-page.js:11-28` disables it against
    double submission. Server requires ≥ 1 photo on Add
    (`Add.cshtml.cs:32-35`).
  - Login lockout: 5 failures lock an account 5 minutes — E2E must use
    throwaway users for wrong-password tests.
  - Culture is forced `pl-PL` (`Booker/Program.cs:112-114`): rendered prices
    use comma decimals (e.g. `12,50`) — assertions must expect that.
  - Seeded item photos point at `images.unsplash.com`
    (`Booker/Data/SeedData.cs:306`) — the suite blocks that origin to avoid
    network flakiness (our own `TestSeed` items use no photos).
  - Rate limiting applies to only 4 identity POST pages
    (`Register`, `ForgotPassword`, `Manage/Email`, `ResendEmailConfirmation`)
    — none of the journeys below POST to those more than a couple of times
    per run, so the default limits are safe with the disciplines in Step 4.
- Fixtures from plan 001: `CustomWebApplicationFactory`, `TestSeed`
  (`Password = "TestPass123!"`), `S3Recorder`, `FakeEmailSender`; plan 003's
  `AuthHttpClient` is NOT used here (we log in through the UI).
- No node/npm anywhere in the repo; we use **Microsoft.Playwright for .NET**
  so everything stays in the dotnet toolchain. The package emits a
  `playwright.ps1`/`playwright.sh` bootstrapper into the build output for
  browser installation.
- Boot pattern (from Microsoft's documented recipe for hosting a real
  server from `WebApplicationFactory`, used in their gRPC testing guidance):
  override `CreateHost` to build a dummy host for the factory machinery,
  rebuild with Kestrel bound to `http://127.0.0.1:0`, start it, and read the
  bound address from `IServerAddressesFeature`.

## Commands you will need

| Purpose | Command | Expected on success |
|---|---|---|
| Build all | `dotnet build Booker/Booker.sln --nologo` | exit 0 |
| Build E2E | `dotnet build Booker.E2e/Booker.E2e.csproj --nologo` | exit 0 |
| Install browsers (one-time) | `pwsh Booker.E2e/bin/Debug/net8.0/playwright.ps1 install chromium` (Windows: `powershell` also works) | chromium downloaded |
| Run E2E | `dotnet test Booker.E2e/Booker.E2e.csproj --nologo` | all pass |
| Run unit+integration only | `dotnet test Booker/Booker.sln --nologo` | unaffected by E2E project |

## Scope

**In scope**:
- `Booker.E2e/` — new project folder: `Booker.E2e.csproj`, `E2eWebAppFixture.cs`,
  `PlaywrightFixture.cs`, specs under `Specs/`, `xunit.runner.json` (create all)
- `.github/workflows/build-test.yml` — add an `e2e` job (append; do not restructure the existing job)

**Out of scope**:
- `Booker/Booker.sln` — the E2E project is deliberately NOT added to the
  solution, so plain `dotnet test Booker/Booker.sln` (CI job 1, local devs)
  never tries to launch browsers. E2E runs by naming the project.
- Any production file.
- Registration E2E (email-confirmation dead end — requires reading mail the
  fake records; deferred, see Maintenance notes), Chat (feature-flagged off,
  static mock), and visual/CSS regression.

## Git workflow

- Branch: `feature/tests-e2e`
- Commit message style: "Add Playwright E2E host fixture", "Cover browse
  filters and book tile journeys" — short imperative.
- Do NOT push or open a PR unless the operator instructed it.

## Steps

### Step 1: Create the E2E project

From the repo root:

```bash
dotnet new xunit -o Booker.E2e -n Booker.E2e -f net8.0
rm Booker.E2e/UnitTest1.cs
cd Booker.E2e
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.30
dotnet add package Microsoft.Playwright
dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
cd ..
dotnet add Booker.E2e/Booker.E2e.csproj reference Booker/Booker.csproj
```

Create `Booker.E2e/xunit.runner.json`:

```json
{ "parallelizeTestCollections": false }
```

and make sure the csproj copies it to output:
`<ItemGroup><None Update="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" /></ItemGroup>`.
Serial execution is required: one shared in-memory app + one Chromium.

**Verify**: `dotnet build Booker.E2e/Booker.E2e.csproj --nologo` → exit 0.

### Step 2: The app host fixture (WAF + real Kestrel)

`Booker.E2e/E2eWebAppFixture.cs` — same overrides as plan 001's
`CustomWebApplicationFactory` (reference that file when writing this; keep
the bodies identical) plus the Kestrel host swap:

```csharp
using Amazon.S3;
using Booker.Data;
using Booker.Tests.Infrastructure; // S3Recorder, FakeEmailSender, TestSeed — project reference needed? NO: see note below
```

**Note on reuse**: `Booker.E2e` cannot reference `Booker.Tests` (test
assemblies referencing each other is fragile). Instead COPY the three small
helper files (`FakeEmailSender.cs`, `S3Recorder.cs`, `TestSeed.cs`) from
`Booker.Tests/Infrastructure/` into `Booker.E2e/Infrastructure/`, changing
the namespace to `Booker.E2e.Infrastructure`. If the copies drift later,
tests fail loudly — acceptable for 3 small files; a shared "Booker.TestUtils"
project is the future fix if that hurts.

```csharp
using Booker.Data;
using Booker.E2e.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Booker.E2e;

public sealed class E2eWebAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    public string BaseUrl { get; private set; } = "";
    public S3Recorder S3 { get; } = new();
    public FakeEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseUrls("http://127.0.0.1:0");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.AddDbContext<DataContext>(options => options.UseSqlite(_connection));
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<Lazy<IAmazonS3>>();
            services.AddSingleton(new Lazy<IAmazonS3>(S3.BuildClient));
        });
    }

    // Host the app on a real Kestrel socket instead of the in-memory TestServer:
    // build the dummy host WebApplicationFactory expects, then a live one to serve.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dummyHost = builder.Build();
        builder.ConfigureWebHost(web => web.UseKestrel());
        var host = builder.Build();
        host.Start();
        BaseUrl = host.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
            .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()!
            .Addresses.First();
        return dummyHost;
    }

    public async Task InitializeAsync()
    {
        _connection.Open();
        CreateClient(); // forces host creation; populates BaseUrl
        var school = await TestSeed.CreateSchoolAsync(Services, "E2E School", "e2e.edu.pl");
        await TestSeed.CreateUserAsync(Services, "e2euser", "e2euser@e2e.edu.pl", school);
        await TestSeed.CreateUserAsync(Services, "e2eother", "e2eother@e2e.edu.pl", school);
        await TestSeed.CreateItemAsync(Services, ownerId: (await FindUserAsync("e2eother")), price: 12.50m);
    }

    private async Task<int> FindUserAsync(string name)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Booker.Data.User>>();
        var user = await userManager.FindByNameAsync(name);
        return user!.Id;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _connection.Dispose();
    }
}
```

Adjust to compile (usings, `UserManager` namespace
`Microsoft.AspNetCore.Identity`). If `BaseUrl` stays empty or the port is
not reachable, see STOP conditions.

**Verify**: a temporary `[Fact]` asserting `fixture.BaseUrl.StartsWith("http://127.0.0.1:")`
passes, and `new HttpClient().GetAsync(fixture.BaseUrl + "/")` returns 200.

### Step 3: The Playwright fixture

`Booker.E2e/PlaywrightFixture.cs` — one browser, one context per spec class
via a small helper:

```csharp
using Microsoft.Playwright;

namespace Booker.E2e;

public sealed class PlaywrightBrowser : IAsyncDisposable
{
    private readonly IPlaywright _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
    private readonly IBrowser _browser;

    public PlaywrightBrowser() =>
        _browser = _playwright.Chromium.LaunchAsync(new() { Headless = true }).GetAwaiter().GetResult();

    public Task<IBrowserContext> NewContextAsync(string? storageStatePath = null) =>
        _browser.NewContextAsync(new() { StorageStatePath = storageStatePath, Locale = "pl-PL" });

    public async ValueTask DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}
```

Plus a `BrowserPage` helper wrapping context+page+`route blocking` of
`**images.unsplash.com**` (abort) so seeded demo photos never hit the
network, and a `LoginAsync(page, "e2euser@e2e.edu.pl")` helper that fills the
real login form and waits for the header to show the username, then (option,
used once by an auth-setup spec) saves
`context.StorageStateAsync("auth/e2euser.json")` for reuse.

**Verify**: a temporary spec loads `/`, asserts `page.TitleAsync()` non-empty
and the network-blocked page still renders → pass.

### Step 4: Write the spec classes (in `Booker.E2e/Specs/`)

All specs use `IClassFixture<E2eWebAppFixture>` + a static shared
`PlaywrightBrowser` (xUnit serializes classes per `xunit.runner.json`).
Journeys, in priority order (one class each):

1. `HomeJourney` — home loads; a category chip click navigates to
   `/Browse` with the expected query parameter (the `938be4f` /
   `b0e54e4` territory).
2. `BrowseJourney` — open `/Browse`; change the grade select; wait for the
   `.grid-gallery` swap (`await page.WaitForLoadStateAsync()` or poll for a
   stable child count — do not assert exact item counts, data is seeded but
   pagination shows 25); assert `page.Url` contains the param; the search
   input triggers the debounced update. Price-filter comma test goes here
   but is `[Fact(Skip = "Red on main until fix/k6-invariant-price-binding (e009560) merges")]`.
3. `BookTileJourney` — from `/Browse`, the first tile's cover `<img>` is
   wrapped by/inside an `<a href^="/Book?id=">` and clicking it lands on the
   detail page (the `769ef06` regression, issue #56).
4. `LoginJourney` — wrong password on a THROWAWAY user (create
   `e2evictim` in `InitializeAsync`) shows the generic error and stays on
   the form; correct login as `e2euser` shows the username in the header;
   logout returns to anonymous header.
5. `BookDetailJourney` — as `e2euser`: contact-reveal button (`hx-get
   ?handler=Email`) swaps in the contact partial and it contains the
   seller's e-mail; favorites button (`hx-post`) toggles Add→Remove via
   `outerHTML` swap; `/Profile/Favorites` then lists the item title.
6. `AddDialogJourney` — as `e2euser`: open `/Add`; open `#summaryModal` and
   close it → dialog closes with **zero** POSTs (count via
   `page.Request` events filtered to method POST); reopen, upload a real
   image, double-click `#confirmAddBtn` → exactly ONE POST to `/Add`;
   assert success (redirect/profile shows the item) and `fixture.S3.Puts`
   grew by the uploaded count. Real image bytes: embed a 1×1 JPEG as a byte
   array constant and `page.SetInputFilesAsync("input[type=file]",
   new FilePayload { Name = "cover.jpg", MimeType = "image/jpeg", Buffer = jpegBytes })`;
   then wait for `aria-busy="false"` on the form before submitting (canvas
   compression is async). The image input's selector must be read from
   `Booker/Pages/Add.cshtml` — pin it by `input[type=file]` or its `name`
   attribute (`Input.Images`).
7. `AdminHidingJourney` — anonymous and `e2euser` GET `/Admin/Index` →
   response status 404 (assert via `page.Response`/`page.GotoAsync` return).
8. `OwnerReserveJourney` (small) — as `e2eother` (owner of the seeded
   item): toggle reserve checkbox → `HX-Refresh` reloads the page with the
   reserved state visible.

Assertions use `pl-PL` formatting for prices (`12,50`). Prefer
`await Expect(locator).ToHaveTextAsync(...)` (auto-retry) over sleeps.

**Verify**: `dotnet test Booker.E2e/Booker.E2e.csproj --nologo` → all pass
locally (requires browsers installed from the commands table).

### Step 5: CI job

Append to `.github/workflows/build-test.yml` (keep the existing
`build-test` job untouched):

```yaml
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - name: Restore
        run: dotnet restore Booker.E2e/Booker.E2e.csproj
      - name: Build
        run: dotnet build Booker.E2e/Booker.E2e.csproj --nologo
      - name: Install Playwright browsers
        run: pwsh Booker.E2e/bin/Debug/net8.0/playwright.ps1 install --with-deps chromium
      - name: Run E2E
        run: dotnet test Booker.E2e/Booker.E2e.csproj --no-build --nologo
```

**Verify**: workflow file parses (eyeball indentation against the existing
job) and the local command from the commands table is the exact thing CI
runs.

## Test plan

This plan IS a test plan: ≥ 15 E2E cases across the 8 journey classes.
Every case maps to a shipped regression (cited per class above) or a core
happy path.

## Done criteria

- [ ] `dotnet build Booker/Booker.sln --nologo` exits 0 (unchanged)
- [ ] `dotnet test Booker/Booker.sln --nologo` exits 0 (E2E project NOT in sln — verify `dotnet sln Booker/Booker.sln list` shows only Booker + Booker.Tests)
- [ ] `dotnet test Booker.E2e/Booker.E2e.csproj --nologo` exits 0 locally with ≥ 15 passing, exactly 1 skipped (comma price)
- [ ] `.github/workflows/build-test.yml` has the `e2e` job
- [ ] `git status` shows no production file modified
- [ ] `plans/README.md` status row updated

## STOP conditions

- The `CreateHost`/Kestrel pattern fails to produce a reachable `BaseUrl`
  (empty addresses, connection refused after start) — report; do not fall
  back to spawning `dotnet run` as an external process without approval.
- The Add page's file input or modal ids differ from `#summaryModal` /
  `#confirmAddBtn` / `input[type=file]` — read `Booker/Pages/Add.cshtml`,
  adjust selectors to reality, and note the difference; if the FLOW changed
  (e.g. no modal), stop.
- Any spec passes only after adding `Task.Delay` sleeps > 2 s — that is a
  synchronization bug in the spec; use auto-retrying `Expect(...)` or event
  waits instead.
- The lockout throwaway user starts failing login specs mid-run (lockout is
  5 attempts / 5 min): keep wrong-password attempts ≤ 3 per run; if a spec
  legitimately needs more, stop and redesign rather than disabling lockout.

## Maintenance notes

- The helper-copy duplication (`Booker.Tests/Infrastructure` →
  `Booker.E2e/Infrastructure`) is deliberate to avoid test-project
  cross-references. If a third consumer appears, extract
  `Booker.TestUtils` and reference it from both.
- Registration E2E is deferred: with `RequireConfirmedAccount = true` and a
  recording `FakeEmailSender`, the confirmation URL can be parsed from
  `Emails.Sent` — a natural follow-up spec when someone prioritizes the
  register journey.
- When `Features:MessagesEnabled` flips on (Chat), add its spec then; today
  the only meaningful assertion is that the nav link is absent while the
  flag is false.
- The rate limiter (10 non-GET/min/IP on 4 identity pages) is not hit by
  these journeys, but new specs POSTing to Register/ForgotPassword must
  budget for it. If E2E ever needs heavy POST volume there, propose a
  config-driven `PermitLimit` rather than editing the policy ad hoc.
- CI runtime: browsers install ~1 min once per run; acceptable. If it
  becomes painful, cache `~/.cache/ms-playwright` keyed on the package
  version.
