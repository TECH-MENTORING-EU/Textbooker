# Plan 003: Integration regression tests for page handlers (auth oracle, privacy, cross-school, ownership)

> **Executor instructions**: Follow this plan step by step. Run every
> verification command and confirm the expected result before moving to the
> next step. If anything in the "STOP conditions" section occurs, stop and
> report — do not improvise. When done, update the status row for this plan
> in `plans/README.md` — unless a reviewer dispatched you and told you they
> maintain the index.
>
> **Drift check (run first)**: `git diff --stat 32efc8d..HEAD -- Booker/Areas/Identity/Pages/Account/Login.cshtml.cs Booker/Pages/Book.cshtml.cs Booker/Pages/Profile/Favorites.cshtml.cs Booker/Pages/Edit.cshtml.cs`
> If any in-scope file changed since this plan was written, compare the
> "Current state" excerpts against the live code before proceeding; on a
> mismatch, treat it as a STOP condition.

## Status

- **Priority**: P1
- **Effort**: L
- **Risk**: LOW (tests only; no production code changes)
- **Depends on**: plans/001-test-foundation.md
- **Category**: tests
- **Planned at**: commit `32efc8d`, 2026-08-26

## Why this matters

Of the ~19 regression fixes audited in git history, **nine were bugs in page
handlers** — every security and privacy regression the project has had lives
at this layer: the login error-message oracle and lockout redirect leak
(commit `e0dfa27`), favorites served for private users (`06673e4`), the phone
number shown despite `DisplayPhone=false` (`b6dc0d9`), cross-school leaks
through all three Book handlers (`680a224`), and delete authorization checked
against the wrong resource (`a7cd534`). All of them are reproducible with
plain HTTP requests against a booted app — exactly what
`WebApplicationFactory` gives us. This plan builds the regression net over
those surfaces. It also lands two deliberately-skipped tests documenting the
live price-binding bug (see "Current state" last bullet).

## Current state

Fixtures from plan 001: `CustomWebApplicationFactory` (SQLite in-memory,
`Testing` env, fake S3/email), `TestSeed` (`CreateSchoolAsync`,
`CreateUserAsync`, `CreateItemAsync`, `MakeAdminAsync`,
`Password = "TestPass123!"`).

Handler contracts verified at `32efc8d`:

- **Login** — `Booker/Areas/Identity/Pages/Account/Login.cshtml.cs`:
  POST `/Identity/Account/Login` with form fields `Input.Email`,
  `Input.Password`, `Input.RememberMe`, `__RequestVerificationToken`.
  Unknown email, wrong password, unconfirmed account and (outside
  Development) lockout all end in `ModelState.AddModelError(string.Empty,
  GenericLoginFailureMessage)` → HTTP 200 with the same generic text
  (lines ~120-170). Success → 302. Identity config: lockout after 5 failed
  attempts for 5 minutes (`Booker/Program.cs:84-86`).
- **Book page** — `Booker/Pages/Book.cshtml.cs`:
  - `OnGetAsync(int id)` lines 22-57: item resolved via
    `itemManager.GetItemAsync(id, currentUser)` → cross-school or missing →
    `NotFound()`; hidden item (`IsVisible == false`) without owner/admin →
    `NotFound()` (lines 39-44).
  - `OnGetEmailAsync(int id)` lines 59-83: same isolation (`item == null` →
    404); anonymous → `204` + `HX-Redirect` header to login; own item →
    `204`; otherwise `Partial("_ContactDetails", BookItem.User)` — the
    partial renders phone/e-mail per privacy flags.
  - `OnPostReserveAsync(int id, bool reserve)` lines 85-106: cross-school →
    404; non-owner → `Forbid()` (403); owner → `204` + `HX-Refresh`.
- **Favorites** — `Booker/Pages/Profile/Favorites.cshtml.cs` lines 30-65:
  GET `/Profile/Favorites?id={id}`; `id` omitted + anonymous → 302 to login;
  `id` omitted + authenticated → own favorites; unknown user → 404; other
  user with `!(IsVisible && AreFavoritesPublic)` → 404 (anti-enumeration
  comment at lines 57-59); self or public → 200.
- **Edit** — `Booker/Pages/Edit.cshtml.cs` lines 25-46: `[Authorize]` page;
  anonymous GET → 302 to login (cookie event); authenticated non-owner →
  `Forbid()` → 403; owner → 200. (Authorization via `ItemOperations.Update`
  and `ItemIsOwnerAuthorizationHandler`, `Booker/Authorization/`.)
- **Admin area** — folder authorized with `AdminHidden`
  (`Booker/Services/StartupUtilities.cs:197-203`); non-admins get **404**, not
  403/302 (`ConfigureAuthorization`, lines 205-241).
- **User privacy flags** — `Booker/Data/User.cs`: `IsVisible`,
  `AreFavoritesPublic`, `DisplayPhone` (set through the `configure` callback
  of `TestSeed.CreateUserAsync`).
- **Live bug, tests must be `Skip`-marked**: decimal binding follows the
  value provider's culture — query strings bind invariantly (`12,50` → 1250
  in Browse price filters) while form fields bind pl-PL (`12.50` → binding
  error in Add/Edit). The fix exists ONLY on local unmerged branch
  `fix/k6-invariant-price-binding` (commit `e009560`,
  `Booker/ModelBinding/InvariantDecimalModelBinder.cs` — not on main).
  Write the tests asserting the CORRECT behavior but mark
  `[Fact(Skip = "...")]` so CI stays green until that branch merges; the Skip
  reason must name the branch.

## Commands you will need

| Purpose | Command | Expected on success |
|---|---|---|
| Build | `dotnet build Booker/Booker.sln --nologo` | exit 0 |
| Run integration suite | `dotnet test Booker/Booker.sln --nologo --filter "FullyQualifiedName~Integration"` | all pass |
| Run all | `dotnet test Booker/Booker.sln --nologo` | all pass |

## Suggested executor toolkit

- None required.

## Scope

**In scope** (create only, under `Booker.Tests/Integration/`):
- `AuthHttpClient.cs` (login helper)
- `LoginOracleTests.cs`
- `BookPageIsolationTests.cs`
- `FavoritesPrivacyTests.cs`
- `EditOwnershipTests.cs`
- `AdminHidingTests.cs`
- `PriceBindingTests.cs`

**Out of scope**:
- Any production file. If an expectation mismatches reality, that is a STOP,
  not an edit.
- HTMX partial content details beyond what the assertions below describe —
  deeper UI behavior belongs to plan 005 (Playwright).
- Photo upload validation — plan 004.

## Git workflow

- Branch: `feature/tests-page-handlers`
- Commit message style: "Add login oracle regression tests", "Cover Book
  handler school isolation" — short imperative.
- Do NOT push or open a PR unless the operator instructed it.

## Steps

### Step 1: The login helper

`Booker.Tests/Integration/AuthHttpClient.cs` — logs in through the real form
(antiforgery included), returns the cookie-bearing client:

```csharp
using System.Text.RegularExpressions;
using Booker.Tests.Infrastructure;

namespace Booker.Tests.Integration;

public static class AuthHttpClient
{
    private static readonly Regex TokenRegex =
        new("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", RegexOptions.Compiled);

    /// <summary>Creates an authenticated client by posting the real login form.</summary>
    public static async Task<HttpClient> LoginAsync(
        CustomWebApplicationFactory factory, string email, string password = TestSeed.Password)
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false,
        });

        var formPage = await client.GetStringAsync("/Identity/Account/Login");
        var token = TokenRegex.Match(formPage).Groups[1].Value
            ?? throw new InvalidOperationException("antiforgery token not found on login page");

        var response = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        }));
        if ((int)response.StatusCode != 302)
        {
            throw new InvalidOperationException($"login failed: {(int)response.StatusCode}");
        }
        return client;
    }
}
```

Add a smoke test inside `LoginOracleTests` that logs a seeded user in and
GETs `/` — if that base assumption fails, everything else will too.

**Verify**: `dotnet test --filter "FullyQualifiedName~Integration"` → the
login smoke passes.

### Step 2: Login oracle tests

Seed one confirmed user; one user with `EmailConfirmed = false`; one
throwaway user for lockout. POST the login form (helper without the 302
assertion — inline the POST or add a `TryLoginAsync` variant returning the
response) for:

1. unknown email,
2. known email + wrong password,
3. known email of unconfirmed user + correct password,
4. known email + correct password AFTER 5 failed attempts (locked out; the
   `Testing` environment is not Development, so the Lockout-page redirect
   must NOT appear — that was the `e0dfa27` leak).

For each, extract the validation-summary error text from the HTML (the
`<div` carrying `validation-summary` / `text-danger` classes — inspect one
real response to pick a stable selector) and assert all four strings are
EQUAL and none contains the email address or the word "zablokowane"/"locked".

**Verify**: suite green.

### Step 3: Book page isolation tests

Seed `schoolA`/`schoolB`, `userA`(A), `userB`(B), an item owned by `userB`,
and a hidden item owned by `userA`'s schoolmate (or by `userA` — visibility
is independent of ownership).

- anonymous GET `/Book?id={itemB}` → 200 (anonymous sees all schools).
- `userA` GET `/Book?id={itemB}` → 404; `?id={itemB}&handler=Email` → 404;
  POST `?id={itemB}&handler=Reserve` (needs antiforgery token — fetch the
  page first; cross-school 404 happens before model validation issues matter
  — if antiforgery rejects first with 400, use the token from `/Book?id=`
  of any accessible page) → 404.
- `userB` GET own-school item → 200 and body contains the book title.
- `userB` GET `?handler=Email` (with request header `HX-Request: true`) →
  200 and partial contains the seller email; create a second user `userC`
  in school B with `DisplayPhone = false` and a `PhoneNumber` set, item
  owned by `userC`: partial must NOT contain the phone digits (the `b6dc0d9`
  regression).
- anonymous `?handler=Email` on a visible item → 204 with `HX-Redirect`
  response header pointing at the login page.
- hidden item: `userB` (non-owner, same school) GET → 404; owner GET → 200.

**Verify**: suite green.

### Step 4: Favorites privacy tests

Seed `userPriv` (`AreFavoritesPublic = false`), `userPub`
(`AreFavoritesPublic = true`, `IsVisible = true`), `userHidden`
(`IsVisible = false`), `userOther` (the requester), one visible item.

- `userOther` GET `/Profile/Favorites?id={userPriv}` → 404.
- `userOther` GET `/Profile/Favorites?id={userHidden}` → 404.
- `userOther` GET `/Profile/Favorites?id={userPub}` → 200.
- `userPub` GET `/Profile/Favorites` (no id) → 200 (own).
- anonymous GET `/Profile/Favorites` → 302 to login.
- anonymous GET `/Profile/Favorites?id={userPub}` → 302 (still requires
  auth per page `[Authorize]`-level behavior — verify by reading the page's
  attributes first; if public favorites are reachable anonymously, assert
  that instead and note it).

**Verify**: suite green.

### Step 5: Edit ownership + admin hiding tests

- anonymous GET `/Edit?id={itemA}` → 302 login.
- `userB` (non-owner) GET `/Edit?id={itemA}` → 403 (`Forbid()`).
- `userA` GET `/Edit?id={itemA}` → 200.
- `userA` GET `/Edit?id=999999` → 404.
- Admin: anonymous + `userA` (non-admin) GET `/Admin/Index` → 404;
  seeded admin (via `TestSeed.MakeAdminAsync`) GET `/Admin/Index` → 200.

**Verify**: suite green.

### Step 6: Price binding tests (Skip-marked, live bug)

Two tests asserting intended behavior, both with
`Skip = "Red on main until fix/k6-invariant-price-binding (e009560) merges"`:

- Add-form culture: authenticated `userA` POSTs `/Add` with
  `Input.Price = "12.50"` (multipart or form-urlencoded with antiforgery) —
  intended: item created with `Price == 12.50m`.
- Query culture: GET `/Browse?MinPrice=12%2C50&MaxPrice=12%2C50` — intended:
  items priced 12.50 are matched (today the comma is silently parsed as
  1250 by the invariant query binder).

Full body included, Skip-marked — they document the contract the pending fix
must satisfy and are un-skipped when it merges.

**Verify**: suite green (2 skipped, visible in the run summary).

## Test plan

This plan IS a test plan; ≥ 20 cases across steps 2-6. Structure follows
plan 001's `SmokeTests` (primary-constructor class injection +
`IClassFixture<CustomWebApplicationFactory>`). One factory per test class —
deliberate, see plan 001 maintenance notes.

## Done criteria

- [ ] `dotnet build Booker/Booker.sln --nologo` exits 0
- [ ] `dotnet test Booker/Booker.sln --nologo` exits 0; ≥ 20 new cases pass, exactly 2 skipped with the k6 Skip reason
- [ ] `git status` shows no production file modified
- [ ] `plans/README.md` status row updated

## STOP conditions

- Any "Current state" excerpt mismatches the live code (drift since `32efc8d`).
- A tested expectation fails against unmodified main for a case NOT marked
  Skip — report which case; do not weaken the assertion to make it pass.
- The login form post cannot achieve 302 for a confirmed seeded user after
  checking `Input.*` field names against `Login.cshtml` markup.
- `HX-Redirect` on anonymous `handler=Email` is absent — means the handler
  changed shape since planning.

## Maintenance notes

- When `fix/k6-invariant-price-binding` merges, remove both Skip attributes
  — the tests must then pass; if they do not, the fix is incomplete.
- The lockout test consumes its throwaway user (5 failed attempts → 5 min
  lockout). Because each test class gets a fresh factory/database, this is
  safe — but do not move these tests onto a shared collection fixture
  without reworking that.
- If `RequireConfirmedAccount` is ever flipped for a test environment via
  config, the unconfirmed-account oracle case loses meaning — revisit.
