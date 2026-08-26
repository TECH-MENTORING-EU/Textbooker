# Plan 002: Characterization tests for ItemManager (school isolation, filtering, item lifecycle)

> **Executor instructions**: Follow this plan step by step. Run every
> verification command and confirm the expected result before moving to the
> next step. If anything in the "STOP conditions" section occurs, stop and
> report — do not improvise. When done, update the status row for this plan
> in `plans/README.md` — unless a reviewer dispatched you and told you they
> maintain the index.
>
> **Drift check (run first)**: `git diff --stat 32efc8d..HEAD -- Booker/Services/ItemManager.cs`
> If the file changed since this plan was written, compare the
> "Current state" excerpts against the live code before proceeding; on a
> mismatch, treat it as a STOP condition.

## Status

- **Priority**: P1
- **Effort**: M
- **Risk**: LOW (tests only; no production code changes)
- **Depends on**: plans/001-test-foundation.md
- **Category**: tests
- **Planned at**: commit `32efc8d`, 2026-08-26

## Why this matters

`ItemManager` is the highest-churn module in the codebase (39 of the 92
commits touching `Booker/Services/`), and its history contains three fixed
regressions: a cross-school data leak because `GetItemAsync` had no school
filter (commit `680a224`), a null-reference crash from a missing
`.ThenInclude(u => u.School)` (commit `6508a31`), and a swallowed
`DbUpdateException` in view tracking (commit `240c831`, fix currently only on
an unmerged branch — on main the race still 500s the book page; see
Maintenance notes). Today a refactor of this file is verified only by manual
clicking. This plan pins its current behavior with characterization tests so
future refactors — including merging the pending fixes — land against a net.

## Current state

`Booker/Services/ItemManager.cs` — 496 lines, primary constructor:

```csharp
public class ItemManager(DataContext context, StaticDataManager staticDataManager, PhotosManager photosManager, ILogger<ItemManager> logger)
```

Everything is injected, so the class is directly constructible in a unit
test — no host needed. Key members to cover:

- `GetItemAsync(int id)` (lines 46-52) — admin overload, no school filter,
  `.Include(i => i.User).ThenInclude(u => u.School)`.
- `GetItemAsync(int id, User? currentUser)` (lines 58-94) — school isolation:
  anonymous → item returned; `currentUser.SchoolId.HasValue` and item's user
  in another school → `null`; user without school sees only items of users
  without a school (lines 84-91).
- `FilterByUserSchool` (lines 401-414) — same rules as query filter for list
  endpoints (`GetAllItemsAsync`, `GetItemIdsByParamsAsync`, ...).
- `ApplyFilters` + helpers (lines 416-459) — search `Contains(search.ToLower())`,
  grades `Any(g => grades.Contains(g))`, subject/level by Id, price bounds.
- `TrackViewAsync` (lines 181-190) — checks `ItemViews.AnyAsync` then inserts;
  second sequential call must be a no-op.
- `ValidateItemModelAsync` (lines 195-225) — flags enum validation against the
  static catalog via `StaticDataManager`; note line 214:
  `!grades.SequenceEqual(model.Parameters.Grades)` — grade equality is
  **order-sensitive** (characterization: document it, don't fix it).
- `AddItemAsync` (lines 227-271) — on `ImageStreams` present: validates
  payload shape (streams/extensions count match, readable, lines 469-495),
  uploads via `photosManager.AddPhotoAsync`, joins keys with `;` into
  `Item.Photo`; else passes `ExistingImageFileNames` through.
- `UpdateItemAsync` (lines 282-340) — new streams replace the whole `Photo`
  string; `ExistingImageFileNames` otherwise; transaction around the write.
- `DeleteItemAsync` (lines 348-367) — deletes row first, then
  `photosManager.DeletePhotosAsync(PhotosManager.StorageKeys(item.Photo))`
  with bare storage keys only.
- `GetPhotosUrl(Item)` (lines 461-467) — splits `Photo` on `;`, maps through
  `photosManager.GetPhotoUrl`.

Dependencies for construction in tests:

- `DataContext` — build directly on SQLite in-memory (see Step 1).
- `StaticDataManager(DataContext context, IMemoryCache cache)` — real
  instance with a fresh `MemoryCache`; reads the `HasData` book catalog.
- `PhotosManager(ILogger<PhotosManager>, Lazy<IAmazonS3>, IConfiguration)` —
  real instance over the NSubstitute-backed `S3Recorder` from plan 001 plus
  an in-memory configuration providing `S3:BucketName` and `CF:PublicUrl`.
- `ILogger<ItemManager>` — `NullLogger<ItemManager>.Instance`.

Fixtures from plan 001 to reuse: `Booker.Tests.Infrastructure.S3Recorder`.
The `IAsyncEnumerable` returns are materialized with `await foreach` or
`.ToListAsync()` (`System.Linq.Async` 6.0.3 flows transitively from the app
project — add `using System.Linq;` and it resolves).

## Commands you will need

| Purpose | Command | Expected on success |
|---|---|---|
| Build | `dotnet build Booker/Booker.sln --nologo` | exit 0 |
| Run only this suite | `dotnet test Booker/Booker.sln --nologo --filter "FullyQualifiedName~ItemManagerTests"` | all pass |
| Run all | `dotnet test Booker/Booker.sln --nologo` | all pass |

## Scope

**In scope**:
- `Booker.Tests/Services/ItemManagerTests.cs` (create)
- `Booker.Tests/Services/ItemManagerTestHost.cs` (create — shared fixture for this class)

**Out of scope**:
- `Booker/Services/ItemManager.cs` — read-only in this plan. If a test
  reveals a genuine bug, do NOT fix it here; report it (characterization
  first, fix in its own PR).
- `Booker/Data/DataContext.cs` — never call `DataContext.CreateBook`; its
  static state poisons other test hosts.
- Page handlers (`Booker/Pages/*.cshtml.cs`) — covered by plan 003.

## Git workflow

- Branch: `feature/tests-item-manager`
- Commit message style: "Add ItemManager characterization tests", then
  "Cover item photo lifecycle in ItemManager tests" — short imperative.
- Do NOT push or open a PR unless the operator instructed it.

## Steps

### Step 1: Create the test host (SQLite context + real managers over fakes)

`Booker.Tests/Services/ItemManagerTestHost.cs`:

```csharp
using Booker.Data;
using Booker.Services;
using Booker.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Booker.Tests.Services;

/// <summary>Owns one SQLite in-memory database and the manager graph over it.</summary>
public sealed class ItemManagerTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public DataContext Context { get; }
    public S3Recorder S3 { get; } = new();
    public ItemManager Items { get; }
    public StaticDataManager StaticData { get; }

    public ItemManagerTestHost()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new DataContext(options);
        Context.Database.EnsureCreated(); // applies HasData book catalog

        StaticData = new StaticDataManager(Context, new MemoryCache());

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["S3:BucketName"] = "test-bucket",
                ["CF:PublicUrl"] = "https://cdn.test",
            })
            .Build();
        var photos = new PhotosManager(
            NullLogger<PhotosManager>.Instance,
            new Lazy<Amazon.IAmazonS3>(S3.BuildClient),
            config);

        Items = new ItemManager(Context, StaticData, photos, NullLogger<ItemManager>.Instance);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        _connection.Dispose();
    }
}
```

**Verify**: `dotnet build Booker/Booker.sln --nologo` → exit 0.

### Step 2: Seed helpers + school-isolation tests

In `ItemManagerTests.cs` (static class, xUnit `[Theory]`s), first add private
seed helpers that create two schools and four users through `Context`
directly: `schoolA`, `schoolB`; `userA` (schoolA), `userB` (schoolB),
`userNoSchool` (null), `userNoSchool2` (null). Then one item per user
(`Item` with `BookId = -1` — a `HasData` book — `UserId`, `Price`, `CreatedAt`).

Write the isolation matrix for `GetItemAsync(id, currentUser)`:

| caller | item owner | expected |
|---|---|---|
| anonymous (`null`) | any | item returned |
| userA | userA | item returned |
| userA | userB (other school) | `null` |
| userA | userNoSchool | `null` |
| userNoSchool | userNoSchool2 | item returned |
| userNoSchool | userA | `null` |
| any | nonexistent id | `null` |

Also assert the admin overload `GetItemAsync(id)` returns cross-school items
and that `item.User.School` is loaded (the `6508a31` regression — accessing
`item.User.School` must not NRE).

**Verify**: `dotnet test ... --filter "FullyQualifiedName~ItemManagerTests"` → green.

### Step 3: List and filter characterization

- `GetAllItemsAsync(user)` / `GetAllItemsCountAsync(user)` follow the same
  school matrix (count assertions).
- `GetItemIdsByParamsAsync` filter matrix against seeded items with distinct
  prices/titles:
  - `Search` matches `Book.Title` case-insensitively (`Contains(search.ToLower())`);
  - `MinPrice`/`MaxPrice` bounds inclusive;
  - `Subject`/`Level`/`Grades` by id — pick values from the `HasData` catalog
    via `StaticData.GetBooksAsync()` so the test uses real ids;
  - combined filters intersect.
- `GetUserItemIdsAsync`/`GetUserItemsCountAsync` scoped to one owner.

**Verify**: same test command → green.

### Step 4: Lifecycle — add, update, delete, reserve, visibility

- `AddItemAsync` success with `ExistingImageFileNames = "key1;key2"` → returns
  `Status.Success` + id; row's `Photo == "key1;key2"`.
- `AddItemAsync` with streams: two small `MemoryStream`s of JPEG magic bytes
  (`0xFF 0xD8 0xFF` + padding) and extensions `[".jpg", ".jpg"]` →
  `S3.Puts.Count == 2`; `Item.Photo` is the two generated keys joined by `;`;
  keys end with `.jpg`.
- `AddItemAsync` payload mismatch (2 streams, 1 extension) → `Status.Error`
  and `S3.Puts` empty.
- Validation matrix (use real catalog values from `StaticData`): non-existent
  title → `InvalidTitle`; wrong subject → has `InvalidSubject|Error`; grades
  in wrong ORDER → has `InvalidGrades|Error` (characterizes the
  `SequenceEqual` at line 214 — add a comment in the test saying this pins
  current behavior); wrong level → `InvalidLevel|Error`.
- `UpdateItemAsync`: with new streams replaces `Photo` (old keys gone from
  the string); with `ExistingImageFileNames` keeps them; price change updates
  `UpdatedAt` (assert `UpdatedAt >= CreatedAt`, not wall-clock equality).
- `MarkItemReservedAsync(id, true)` then `(id, false)` round-trip.
- `TrackViewAsync(itemId, userId)` twice → `GetViewCountAsync == 1`.
- `DeleteItemAsync`: seed item with `Photo = "key1;https://external/img.png;key2.jpg"`;
  after delete the row is gone and `S3.Deletes` received exactly the bare
  keys `["key1", "key2.jpg"]` (this pins `PhotosManager.StorageKeys` filtering —
  the GDPR contract from commit `6ea5f1a`).
- `SetItemsVisibilityByUserAsync(ownerId, false)` hides that owner's items only.

**Verify**: full `dotnet test` green.

## Test plan

This plan IS a test plan; production code is untouched. Target: ≥ 25 test
cases across steps 2-4. Model file organization after
`Booker.Tests/SmokeTests.cs` (primary-constructor class injection, no
`IClassFixture` needed — the host is created per test class instance).

## Done criteria

- [ ] `dotnet build Booker/Booker.sln --nologo` exits 0
- [ ] `dotnet test Booker/Booker.sln --nologo` exits 0; ≥ 25 ItemManager cases pass
- [ ] `git diff --stat 32efc8d..HEAD -- Booker/Services/ItemManager.cs` is empty
- [ ] No production file modified (`git status`)
- [ ] `plans/README.md` status row updated

## STOP conditions

- `Booker/Services/ItemManager.cs` no longer matches the excerpts (drift).
- A characterization test FAILS on unmodified main — that means the plan's
  expected behavior is wrong or the code has a live bug. Do not adjust
  production code; report which case failed.
- `EnsureCreated()` does not seed books (a `HasData` restructure) — pick book
  ids from the catalog at runtime instead of hardcoding `-1`; if the catalog
  itself is gone, stop.

## Maintenance notes

- The grade-order test intentionally documents order-sensitive validation.
  If someone "fixes" `ValidateItemModelAsync` to be order-insensitive, that
  test must be updated with the fix — that is the characterization contract
  working as designed.
- Known live gap (do not fix here): `TrackViewAsync` on main does not guard
  the concurrent-duplicate insert race (the `DbUpdateException` catch exists
  only on unmerged branch `feature/wcag-2.2-aa`, commit `817cb61`). A
  deterministic test for the race needs two concurrent contexts; leave it
  out until that fix merges, then add a test asserting the catch.
- Reviewer focus: no wall-clock assertions (`DateTime.Now` is used inside
  the manager — assert ordering, not values). A `TimeProvider` refactor has
  been considered and deferred (see plans/README.md, rejected findings).
