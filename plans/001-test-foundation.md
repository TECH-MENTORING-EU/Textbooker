# Plan 001: Establish the test foundation — xUnit project, WAF host, CI test gate

> **Executor instructions**: Follow this plan step by step. Run every
> verification command and confirm the expected result before moving to the
> next step. If anything in the "STOP conditions" section occurs, stop and
> report — do not improvise. When done, update the status row for this plan
> in `plans/README.md` — unless a reviewer dispatched you and told you they
> maintain the index.
>
> **Drift check (run first)**: `git diff --stat 32efc8d..HEAD -- Booker/Program.cs Booker/Services/StartupUtilities.cs Booker/Booker.sln`
> If any in-scope file changed since this plan was written, compare the
> "Current state" excerpts against the live code before proceeding; on a
> mismatch, treat it as a STOP condition.

## Status

- **Priority**: P1
- **Effort**: M
- **Risk**: MED (touches startup code; gated strictly on environment/provider)
- **Depends on**: none
- **Category**: tests
- **Planned at**: commit `32efc8d`, 2026-08-26

## Why this matters

The repository has 604 commits, an active stream of regression-fix PRs, and
**zero automated tests**: `Booker/Booker.sln` contains only the app project,
`Booker.Tests/` holds nothing but stale `bin/`/`obj/` from a deleted branch,
`dotnet test` is a no-op, and CI (`.github/workflows/deploy.yml`) only builds
and deploys on `push` to `release` — nothing ever verifies a change before it
merges. Every one of the ~19 regression fixes audited in git history was
caught by manual clicking. This plan creates the one-command verification
baseline every later plan builds on: an xUnit project, a
`WebApplicationFactory` host that boots the real app on SQLite in-memory with
fake email/S3 adapters, and a CI workflow that runs build + test on every PR.

## Current state

- `Booker/Booker.sln` — solution with exactly one project (`Booker.csproj`).
  `dotnet test Booker/Booker.sln` prints restore output and exits 0 having run
  nothing.
- `Booker.Tests/` — untracked leftovers (`bin/`, `obj/`) from an unmerged
  branch. There is no `Booker.Tests.csproj`. We will create one here.
- `Booker/Program.cs:68-72` — the DbContext is hardwired to SqlServer:
  ```csharp
  builder.Services.AddDbContext<DataContext>(options =>
  {
      //options.UseInMemoryDatabase("InMemoryDatabaseName");
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseCompatibilityLevel(110));
  });
  ```
  `Booker/Program.cs` uses top-level statements and ends (line 167) with
  `app.Run();` — there is **no** `public partial class Program { }`, so
  `WebApplicationFactory<Program>` cannot compile from another assembly.
- `Booker/Program.cs:157` — `await app.MigrateDatabaseAsync(configuration);`
  runs during host start.
- `Booker/Services/StartupUtilities.cs:277-323` — `MigrateDatabaseAsync`
  resolves EF's `IMigrator` and calls `MigrateAsync()`. Migrations are
  SqlServer-specific: against SQLite they throw. The exception is swallowed
  (`catch { LogError }` at lines 316-319), leaving a schema-less database.
- `Booker/Services/StartupUtilities.cs:21-80` — `AddBookerServices`
  registrations we will override in tests:
  - line 41-57: singleton `Lazy<IAmazonS3>` (real Cloudflare R2 client),
  - line 63: `AddSingleton<IEmailSender, SendMailSvc>()` (real SMTP, failures
    swallowed inside `SendMailSvc.Send`).
- `Booker/Program.cs:159-164` — dev-only seeding
  (`app.InitializeDatabaseAsync()`) runs only `if (app.Environment.IsDevelopment())`.
  We will use a custom `Testing` environment and seed explicitly from the test
  project, keeping test data deterministic (the Development seeding adds ~150
  randomized rows via `Random.Shared`/`Guid.NewGuid` —
  `Booker/Data/SeedData.cs:235-313`).
- `Booker/Data/DataContext.cs` — `OnModelCreating` uses `HasData` for
  `Books`, `Subjects`, `Levels`, `Grades` and the book-grade join table
  (static lists around lines 22-57). `EnsureCreated()` therefore produces a
  usable schema **with** the book catalog pre-seeded. ⚠️ Do NOT call
  `DataContext.CreateBook(...)` anywhere in tests: it mutates process-wide
  statics (`DataContext.cs:19,22,108`) and poisons every other test host.
- `Booker/Services/StartupUtilities.cs:343-365` — `InitializeRolesAsync`
  creates the `Admin` role on every boot (works on any provider; the `a1`
  user promotion is Development-only, irrelevant for `Testing`).
- Baseline build verified during planning: `dotnet build Booker/Booker.sln`
  → 0 warnings, 0 errors (~5 s).

### Known local-state warning

At planning time the operator's working tree had a local uncommitted diff in
`Booker/Program.cs` (Serilog `MinimumLevel.Override` for EF commands set to
`Information` — a leftover from a k6 load-test session). If you see that diff,
do not revert or commit it yourself — leave it uncommitted or ask the
operator; your own changes must not include it.

## Commands you will need

| Purpose | Command | Expected on success |
|---|---|---|
| Build app | `dotnet build Booker/Booker.sln --nologo` | `0 ostrzeżeń / 0 błędów` (Polish: warnings/errors) |
| Build all | `dotnet build` (repo root, picks up sln via Booker.Tests reference — prefer explicit sln path) | exit 0 |
| Run tests | `dotnet test Booker/Booker.sln --nologo` | all pass, > 0 tests run |
| Add project to sln | `dotnet sln Booker/Booker.sln add Booker.Tests/Booker.Tests.csproj` | `Project ... added to the solution` |

## Suggested executor toolkit

- None required. Plain `dotnet` CLI + any editor.

## Scope

**In scope** (the only files you should create/modify):
- `Booker.Tests/Booker.Tests.csproj` (create) and test source files under `Booker.Tests/` (create)
- `Booker/Booker.sln` (add test project)
- `Booker/Program.cs` (two changes: `public partial class Program { }`; nothing else)
- `Booker/Services/StartupUtilities.cs` (one change: provider branch in `MigrateDatabaseAsync`)
- `.github/workflows/build-test.yml` (create)

**Out of scope** (do NOT touch, even though they look related):
- `Booker/Program.cs` DbContext registration — we override the provider from
  the test factory via `RemoveAll<DbContextOptions<DataContext>>()`; do NOT
  make the app's provider config-driven in this plan.
- `.github/workflows/deploy.yml` — production deploy pipeline; leave as is.
- Any file under `Booker/Pages/`, `Booker/Services/` other than
  `StartupUtilities.cs`, `Booker/Data/`.
- Do not "clean up" the stray `nul` file at repo root beyond noting it in
  your report (Windows reserved name; deleting it is the operator's call).

## Git workflow

- Branch: `feature/tests-foundation`
- Commit per step; message style — short imperative, no prefix convention
  (examples from `git log`: "Validate profile picture uploads and set S3
  object content type", "Extract shared image magic-byte detector").
- Do NOT push or open a PR unless the operator instructed it.

## Steps

### Step 1: Create the xUnit test project and add it to the solution

From the repo root:

```bash
cd Booker.Tests && rm -rf bin obj && cd ..
dotnet new xunit -o Booker.Tests -n Booker.Tests -f net8.0
rm Booker.Tests/UnitTest1.cs
dotnet sln Booker/Booker.sln add Booker.Tests/Booker.Tests.csproj
cd Booker.Tests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.30
dotnet add package NSubstitute --version 5.1.0
dotnet add package Microsoft.NET.Test.Sdk --version 17.12.0
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
```

(The template already references Test.Sdk/xunit; the explicit pins keep
versions deterministic. Any 8.0.x of Mvc.Testing is fine if 8.0.11 conflicts.)

Edit `Booker.Tests/Booker.Tests.csproj` to match the app's style and add
`<IsPackable>false</IsPackable>` in the `PropertyGroup`.

**Verify**: `dotnet build Booker/Booker.sln --nologo` → exit 0.

### Step 2: Make `Program` visible to the test assembly

Append one line at the very end of `Booker/Program.cs` (after `app.Run();`):

```csharp
public partial class Program { }
```

This is the standard minimal-hosting hook for
`WebApplicationFactory<Program>`. It changes no runtime behavior.

**Verify**: `dotnet build Booker/Booker.sln --nologo` → exit 0.

### Step 3: Branch `MigrateDatabaseAsync` on the database provider

In `Booker/Services/StartupUtilities.cs`, inside
`MigrateDatabaseAsync` (currently lines 277-323), insert immediately after
`var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();`
(line ~291) and before `var migrator = ...`:

```csharp
// Non-relational/test providers (SQLite in the test host) cannot run the
// SqlServer-specific migrations; build the schema straight from the model.
if (dbContext.Database.IsSqlite())
{
    await dbContext.Database.EnsureCreatedAsync();
    logger.LogInformation("SQLite schema created from the model (test provider).");
    return app;
}
```

Add `using Microsoft.EntityFrameworkCore;` if not already present
(`IsSqlite()` lives there). Production is untouched: SqlServer keeps the
migrator path, and `deploy.yml` never configures SQLite.

**Verify**: `dotnet build Booker/Booker.sln --nologo` → exit 0, and
`git diff Booker/Services/StartupUtilities.cs` shows only this insertion.

### Step 4: Create the fakes and the factory

Create `Booker.Tests/Infrastructure/FakeEmailSender.cs`:

```csharp
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Booker.Tests.Infrastructure;

public sealed class FakeEmailSender : IEmailSender
{
    public List<(string Email, string Subject, string Body)> Sent { get; } = new();

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Sent.Add((email, subject, htmlMessage));
        return Task.CompletedTask;
    }
}
```

Create `Booker.Tests/Infrastructure/S3Recorder.cs` — a recording test double
for storage, built on NSubstitute so we never hand-implement the huge
`IAmazonS3` interface:

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using NSubstitute;

namespace Booker.Tests.Infrastructure;

/// <summary>Records S3 traffic so tests can assert keys, content types and delete calls.</summary>
public sealed class S3Recorder
{
    public List<PutObjectRequest> Puts { get; } = new();
    public List<DeleteObjectsRequest> Deletes { get; } = new();
    /// <summary>Set true to simulate a storage outage during deletes.</summary>
    public bool FailDeletes { get; set; }

    public IAmazonS3 BuildClient()
    {
        var client = Substitute.For<IAmazonS3>();
        client.PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                Puts.Add(callInfo.ArgAt<PutObjectRequest>(0));
                return new PutObjectResponse();
            });
        client.DeleteObjectsAsync(Arg.Any<DeleteObjectsRequest>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var request = callInfo.ArgAt<DeleteObjectsRequest>(0);
                Deletes.Add(request);
                if (FailDeletes)
                {
                    throw new AmazonS3Exception("simulated storage outage");
                }
                return new DeleteObjectsResponse();
            });
        return client;
    }
}
```

If the AWSSDK.S3 4.x response constructors differ (e.g. required init
properties), adapt minimally — the point is only a non-null return value.

Create `Booker.Tests/Infrastructure/CustomWebApplicationFactory.cs`:

```csharp
using Amazon.S3;
using Booker.Data;
using Booker.Services;
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests.Infrastructure;

/// <summary>
/// Boots the real app on the "Testing" environment with a shared SQLite
/// in-memory database, a recording email sender and a fake S3 adapter.
/// Fresh factory per test class: the app's IMemoryCache (StaticDataManager,
/// SessionCacheManager) is per-host, so sharing one factory across classes
/// would leak cached lists and sessions between them.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public S3Recorder S3 { get; } = new();
    public FakeEmailSender Emails { get; } = new();

    public CustomWebApplicationFactory()
    {
        _connection.Open(); // keep open for the host's lifetime: the in-memory DB persists
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Swap the SqlServer DbContext for SQLite on the shared connection.
            services.RemoveAll<DbContextOptions<DataContext>>();
            services.AddDbContext<DataContext>(options => options.UseSqlite(_connection));

            // Never talk to real SMTP from tests.
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);

            // Never talk to real R2/S3 from tests.
            services.RemoveAll<Lazy<IAmazonS3>>();
            services.AddSingleton(new Lazy<IAmazonS3>(S3.BuildClient));
        });
    }

    protected override void Dispose(bool disposing)
    {
        _connection.Dispose();
        base.Dispose(disposing);
    }
}
```

(`RemoveAll` comes from
`Microsoft.Extensions.DependencyInjection.Extensions` — pulled in transitively
by Mvc.Testing. Add the using if the compiler asks.)

Create `Booker.Tests/Infrastructure/TestSeed.cs` — deterministic seed data
(the `Testing` environment runs none of the app's own seeding):

```csharp
using Booker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Booker.Tests.Infrastructure;

public static class TestSeed
{
    public const string Password = "TestPass123!"; // same constant the dev seed uses (Booker/Data/SeedData.cs:18)

    public static async Task<int> CreateSchoolAsync(IServiceProvider services, string name, string domain)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var school = new School { Name = name, EmailDomain = domain, IsActive = true };
        context.Schools.Add(school);
        await context.SaveChangesAsync();
        return school.Id;
    }

    /// <summary>Creates a confirmed, login-able user. Extra property tweaks go through <paramref name="configure"/>.</summary>
    public static async Task<int> CreateUserAsync(
        IServiceProvider services, string userName, string email, int? schoolId,
        Action<User>? configure = null, string password = Password)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true, // RequireConfirmedAccount = true (Program.cs:76)
            SchoolId = schoolId,
            IsVisible = true,
        };
        configure?.Invoke(user);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Seed user creation failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        return user.Id;
    }

    public static async Task<int> CreateItemAsync(
        IServiceProvider services, int ownerId, decimal price = 20m, bool isVisible = true)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var book = await context.Books.FindAsync(-1)
            ?? throw new InvalidOperationException("HasData books missing — EnsureCreated did not run.");
        var item = new Item
        {
            BookId = book.Id,
            UserId = ownerId,
            Description = "seed item",
            State = "dobry",
            Price = price,
            IsVisible = isVisible,
            CreatedAt = DateTime.Now,
            Photo = "",
        };
        context.Items.Add(item);
        await context.SaveChangesAsync();
        return item.Id;
    }

    public static async Task MakeAdminAsync(IServiceProvider services, int userId)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("user missing");
        await userManager.AddToRoleAsync(user, "Admin"); // role exists: InitializeRolesAsync runs on every boot
    }
}
```

Check `Booker/Data/User.cs` and `Booker/Data/Item.cs` for the actual property
names/defaults (`IsVisible`, `SchoolId`, `Photo`, `State`, `CreatedAt`) and
adjust the initializers to whatever compiles — do not invent properties.

Create `Booker.Tests/SmokeTests.cs`:

```csharp
using Booker.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Booker.Tests;

public class SmokeTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Home_page_returns_html()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Browse_page_returns_html()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Browse");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Unknown_book_id_returns_404()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Book?id=999999");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_area_returns_404_for_anonymous()
    {
        // AdminHidden hides the folder as 404, not 403/redirect (StartupUtilities.cs:197-203, 205-241)
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Admin/Index");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Seeded_user_profile_returns_200()
    {
        var userId = await TestSeed.CreateUserAsync(factory.Services, "seedu1", "seedu1@example.edu.pl", schoolId: null);
        var client = factory.CreateClient();
        var response = await client.GetAsync($"/Profile/{userId}");
        Assert.True(response.IsSuccessStatusCode, $"got {(int)response.StatusCode}");
    }
}
```

**Verify**: `dotnet test Booker/Booker.sln --nologo` → 5 tests, all pass.
If `Admin_area_returns_404_for_anonymous` gets 302 instead, read
`StartupUtilities.ConfigureAuthorization` again — the 404 conversion relies on
the `HideUnauthorized` flow; a 302 means the app changed since planning, which
is a STOP condition.

### Step 5: Add the CI workflow

Create `.github/workflows/build-test.yml`:

```yaml
name: Build and test

on:
  pull_request:
  push:
    branches: [main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - name: Restore
        run: dotnet restore Booker/Booker.sln
      - name: Build
        run: dotnet build Booker/Booker.sln -c Release --no-restore --nologo
      - name: Test
        run: dotnet test Booker/Booker.sln -c Release --no-build --nologo
```

Note: the app builds on Linux (verified locally on Windows; the only
platform-specific piece is `AspNetCore.SassCompiler`, which ships per-OS
dart-sass binaries inside the NuGet package and needs no Node). If restore or
SCSS compilation fails on `ubuntu-latest`, switch the runner to
`windows-latest` — it is what `deploy.yml` uses.

**Verify**: `git status` shows the new file; YAML lint is not available
locally, so double-check indentation by eye (2-space, consistent with
`deploy.yml`).

## Test plan

The smoke tests above ARE this plan's test plan. Later plans (002-005) add
the deep coverage; this one only proves the harness works end to end.

## Done criteria

Machine-checkable. ALL must hold:

- [ ] `dotnet build Booker/Booker.sln --nologo` exits 0
- [ ] `dotnet test Booker/Booker.sln --nologo` exits 0 with ≥ 5 tests run and 0 failed
- [ ] `git diff --stat 32efc8d..HEAD -- Booker/Program.cs` shows exactly one added line (`public partial class Program { }`)
- [ ] `git diff --stat 32efc8d..HEAD -- Booker/Services/StartupUtilities.cs` shows only the SQLite branch in `MigrateDatabaseAsync`
- [ ] `.github/workflows/build-test.yml` exists with a `dotnet test` step
- [ ] No files outside the in-scope list are modified (`git status`)
- [ ] `plans/README.md` status row updated

## STOP conditions

Stop and report back (do not improvise) if:

- The committed code at the locations in "Current state" doesn't match the
  excerpts (drift since `32efc8d`).
- `Booker/Program.cs` working diff contains anything other than the known
  Serilog log-level leftover described in "Known local-state warning".
- `Admin_area_returns_404_for_anonymous` returns anything but 404 (see above).
- `TestSeed` cannot compile because `User`/`Item`/`School` property names
  differ materially from the initializers after you checked the entity files —
  adapt names, but if a required concept is missing (e.g. no `IsVisible`),
  stop and report instead of redesigning the seed.
- The AWSSDK 4.x `IAmazonS3` surface makes the NSubstitute double impossible
  to configure for `PutObjectAsync`/`DeleteObjectsAsync`.
- A step's verification fails twice after a reasonable fix attempt.

## Maintenance notes

- Every later test plan (002-005) reuses `CustomWebApplicationFactory`,
  `TestSeed`, `FakeEmailSender` and `S3Recorder`. Renaming or moving these
  breaks four plans — treat them as public API of the test project.
- The per-class-factory design is deliberate: the app caches
  `StaticDataManager` lists for 1 hour with no invalidation
  (`Booker/Services/StaticDataManager.cs:28`) and keeps session state in a
  shared `IMemoryCache` (`SessionCacheManager`), so one shared factory would
  leak state between test classes. If test suite runtime ever becomes a
  problem, optimize with a collection fixture + explicit cache clearing, not
  by sharing blindly.
- `MigrateDatabaseAsync`'s swallowed exceptions
  (`StartupUtilities.cs:316-319`) are pre-existing; this plan routes tests
  away from them (SQLite branch) but does not change production failure
  semantics. If someone later makes migration failures fail-fast, the
  `Testing` environment must be added to the exemption.
- The `dotnet new xunit` template may create `GlobalUsings.cs` or
  `xunit.runner.json` — keep whatever it generates; delete only
  `UnitTest1.cs`.
