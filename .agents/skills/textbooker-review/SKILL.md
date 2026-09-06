---
name: textbooker-review
description: Review and validate Textbooker code changes across ASP.NET Core Razor Pages, EF Core, HTMX, Pico/SCSS, JavaScript, accessibility, configuration, and tests. Use before opening or merging a PR, when auditing a branch diff, or when fixing review findings in this repository.
---

# Textbooker Code Review

Inspect the actual diff and repository state before applying this checklist. Preserve unrelated local changes and generated test artifacts.

## Review order

1. Read `git status --short --branch` and `git diff --check`.
2. Review the branch diff against its real PR base, not automatically against `main`.
3. Trace every behavior change through Razor markup, page model/service, JavaScript, SCSS, configuration, and tests.
4. Fix actionable defects only. Do not implement reviewer suggestions that would regress correctness or accessibility.
5. Run checks proportional to the touched surface.

## Configuration and startup

- Use the injected `builder.Configuration`; do not create a competing configuration pipeline.
- Keep environment-specific database initialization behind explicit configuration. Never delete a database merely because the environment is named `Testing`.
- Gate destructive test setup with `DatabaseSettings:ClearDatabaseOnStartup`; set it explicitly in test configuration when deterministic cleanup is required.
- Encapsulate environment setup, database migration/seeding, development endpoints, and role/test-data initialization in focused `StartupUtilities` extensions instead of growing `Program.cs`.
- Keep test seed data close to database initialization and make it idempotent.
- Do not commit predictable privileged test credentials unless the environment is provably local and ephemeral. Prefer environment variables and fail closed if a `Testing` deployment is externally reachable.
- Verify that `builder.Environment.EnvironmentName`, `app.Environment`, and the configuration instance select the same environment and provider, including with `--environment Testing`.

## Photo storage and URL safety

- Construct S3 clients lazily so missing development credentials do not break application startup.
- Convert storage configuration and transport failures into `PhotoStorageException` at the service boundary; log technical context and return a user-safe validation error.
- Never trust an absolute photo URL merely because `Uri.TryCreate` accepts it.
- Allow only `http`/`https` absolute URLs matching the configured public CDN host and port. Reject `data:`, `javascript:`, foreign hosts, and malformed values.
- Keep the trust condition in a named helper such as `IsTrustedPublicUrl`.
- Preserve safe root-relative application assets such as `/img/default-book.svg`, but reject network-path references beginning `//` or `\\`; browsers interpret them as external URLs that bypass an absolute-URI allowlist.
- Trace stored photo values back to form fields such as existing image names. Treat them as user-controlled even when they normally originate from the server.

## EF Core and data integrity

- Do not drop live columns in the same migration that introduces replacements. Add, backfill, verify, then remove later.
- Keep `Up()`, `Down()`, seed data, and model snapshots consistent.
- Keep soft-delete fields synchronized: set `IsActive = false` with `DeactivatedAt`, and reverse both on reactivation.
- Ensure normal queries exclude inactive rows.
- Align `dotnet-ef` major/minor with EF Core packages.
- Wrap multi-step write operations in an EF transaction (`DbContext.Database.BeginTransaction`) so partial failures roll back. Default read-committed isolation guards against races during create-then-update flows.
- Relying on a primary-key constraint to reject duplicates is optimistic concurrency, not a substitute for a transaction. Make it intentional, not incidental.
- SQL Server rejects multiple cascade paths. When a join entity links two principals that already cascade into it, configure one relationship as `DeleteBehavior.NoAction` and perform explicit cleanup in the owning operation.
- Do not commit local database, Azurite, WAL/SHM, or generated test-result files. Confirm migration artifacts belong to this project and are not leftovers from an older schema.

## Domain invariants

- A listing (`Item`) references catalog metadata (`Book`); do not duplicate catalog data into listings merely to add a feature such as ISBN scanning or view counts.
- One authenticated user contributes at most one view per item. Enforce this with a composite unique key and an intentional concurrency strategy; do not count repeated refreshes as new views.
- A user who hides email must retain another enabled contact method. Keep server-side validation authoritative and show the warning only when every contact channel would be disabled.
- Seeded catalog data and admin-managed catalog data are different concerns. Verify seed assumptions before using `SingleAsync` during test initialization.

## Razor and JavaScript

- Move page-specific scripts out of large inline `<script>` blocks into `wwwroot/js` and load them with `asp-append-version="true"`.
- Avoid coupling behavior to generated Razor IDs when the element itself can provide the value. Prefer event targets or stable `data-*` hooks.
- Guard optional DOM lookups before reading properties.
- Prevent duplicate listener registration when HTMX swaps content.
- Keep table headers, body partials, render parameters, and component parameters structurally aligned.

## Responsive UI

- Use CSS Grid, media queries, and container constraints for layout. Do not count visible cards with `resize` handlers or `window.innerWidth`.
- It is acceptable for Razor to select a bounded data set; viewport-dependent presentation belongs in CSS.
- Test at least mobile, ordinary desktop, and ultra-wide 4K dimensions. On 4K, widen only the landing container and density at an explicit breakpoint; do not stretch content edge to edge.
- For a single-row gallery, control density with `grid-template-columns` and `:nth-child`, then reveal an additional card at the ultra-wide breakpoint.
- Preserve existing branding, navigation labels, focus styles, and reduced-motion behavior.

## Accessibility

- Associate every field with a label and validation message. Dynamic errors should use an appropriate live region or `role="alert"`.
- Give non-submit buttons `type="button"`.
- Keep HTMX status announcements truthful: announce success only when `event.detail.successful` is true.
- Update loading state dynamically; do not leave a static false `aria-busy` value during a request.
- ARIA token values must serialize as lowercase `true`/`false`. JavaScript `setAttribute` safely stringifies booleans; Razor output should be checked in rendered HTML.
- Put filter names and accessible labels in Razor markup when the server already knows them; JavaScript should update state, not reconstruct semantics from text.
- Store filter values in `data-filter-value`; never parse grades with fixed string slices.
- Keep thumbnail selection in `aria-pressed`; do not leave stale “currently selected” text in thumbnail alternatives.
- Ensure dialogs reset form state without assuming the dialog is nested inside the form.

## SCSS and visual details

- Use valid CSS values, including `forced-color-adjust: preserve-parent-color`.
- Use Pico design tokens rather than hard-coded theme colors.
- Avoid `!important` unless a documented cascade constraint requires it.
- Give abbreviated files such as `_a11y.scss` a clear top-level purpose comment.

## Clean code

- Drop explicit `== true` / `== false` comparisons; let the bool convert implicitly.
- Do not initialize bool fields or locals with `false`; the default is already `false`.
- Simplify if-else chains with early returns. Once a branch returns, the trailing `else` / `else if` is dead weight.
- Flatten nested conditionals rather than stacking `if` inside `if`; extract conditions into a named helper when the intent is not obvious.
- Remove leftover blank lines that do not separate logical blocks.

## Naming

- Choose names that describe the value's purpose within the method, not its type or composition.
- Avoid compound names that stitch two domain objects together (e.g. `SchoolUserCount`) when a simpler name (`School`) conveys the same meaning; over-description is a smell, not a virtue.
- Prefer positive boolean names. `includeInactive` reads better than the double negation `!excludeInactive`.

## Logging and warnings

- Match every structured logging placeholder with exactly one argument.
- Prefer structured logging arguments over interpolation.
- Treat new compiler/analyzer warnings as findings. Distinguish inherited warnings in the PR base from warnings introduced by the branch.

## Documentation evidence

- Verify that files, workflows, commands, and test suites claimed by documentation exist in the reviewed branch.
- Do not claim axe/WCAG results before the suite that produced them lands. Mark future stack artifacts as planned and do not use them as current compliance evidence.

## Verification matrix

Run only commands supported by the branch, but do not silently skip relevant checks:

```powershell
git diff --check
dotnet build Booker/Booker.csproj --nologo --verbosity minimal
if (Test-Path Booker.Tests/Booker.Tests.csproj) { dotnet test Booker.Tests/Booker.Tests.csproj }
if (Test-Path package.json) { npm run test:a11y }
```

For browser-test changes, run the full Playwright suite. Use `--list` first to catch discovery/configuration failures. Await `HTMLImageElement.decode()` before declaring an image broken to avoid load-race flakes.

For migrations, additionally run `dotnet ef migrations list` and generate an idempotent script.

Report passed checks, inherited warnings, skipped checks with reasons, and any remaining risk.

When the reviewed branch is not checked out and the main worktree is dirty, stay read-only: inspect with `git diff <base>..<head>`, `git show <head>:<path>`, `git grep <pattern> <head>`, and `git ls-tree -r <head>`. Use an existing clean worktree for builds; do not checkout over user changes.
