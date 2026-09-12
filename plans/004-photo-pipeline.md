# Plan 004: Tests for the photo pipeline (magic bytes, storage keys, GDPR purge, upload validation)

> **Executor instructions**: Follow this plan step by step. Run every
> verification command and confirm the expected result before moving to the
> next step. If anything in the "STOP conditions" section occurs, stop and
> report — do not improvise. When done, update the status row for this plan
> in `plans/README.md` — unless a reviewer dispatched you and told you they
> maintain the index.
>
> **Drift check (run first)**: `git diff --stat 32efc8d..HEAD -- Booker/Services/PhotosManager.cs Booker/Services/ImageFormatDetector.cs Booker/Services/UserPhotoManager.cs Booker/Pages/Shared/ImageUploadValidation.cs Booker/Areas/Admin/Pages/Users.cshtml.cs`
> If any in-scope file changed since this plan was written, compare the
> "Current state" excerpts against the live code before proceeding; on a
> mismatch, treat it as a STOP condition.

## Status

- **Priority**: P2
- **Effort**: M
- **Risk**: LOW (tests only; no production code changes)
- **Depends on**: plans/001-test-foundation.md
- **Category**: tests
- **Planned at**: commit `32efc8d`, 2026-08-26

## Why this matters

Photos are the project's security-sensitive edge: uploads land as
`PublicRead` objects in public Cloudflare R2 storage, and recent history
shows exactly the bugs this pipeline breeds — arbitrary bytes accepted as
profile pictures (commit `8b0e88b`), content type trusted from the file
extension (commit `8d76895`), and account deletion that orphaned profile and
item photos forever (commit `6ea5f1a`, GDPR). The seams are already clean —
`PhotosManager` takes `Lazy<IAmazonS3>` + `IConfiguration` (no client
construction inside business logic), `ImageFormatDetector` is a pure static,
`UserPhotoManager` orchestrates over `DataContext` + `PhotosManager` — so the
whole pipeline is testable with the fakes from plan 001 and no production
changes.

## Current state

- `Booker/Services/ImageFormatDetector.cs` — pure static, `DetectExtension(Stream)`
  returns `".jpg"` for `FF D8` headers, `".png"` for the full 8-byte PNG
  signature, else `null`; resets stream position when seekable (lines 13-49).
- `Booker/Services/PhotosManager.cs` — primary ctor
  `(ILogger<PhotosManager>, Lazy<IAmazonS3>, IConfiguration)`.
  - `AddPhotoAsync(stream, ext)` (line 18+): throws `PhotoStorageException`
    when `S3:BucketName` is unset; key = `Guid.NewGuid() + ext`; `PutObject`
    with `CannedACL.PublicRead` and `ContentType = GetContentType(stream, ext)`
    (content type resolved by magic bytes, extension normalized — commits
    `8d76895`/`cb6e828`).
  - `StorageKeys(...)` / `GetPhotoUrl(...)` (lines ~132-157): pure helpers —
    `StorageKeys` filters a `;`-joined `Photo` string down to bare storage
    keys only (absolute URLs and root-relative assets are skipped); used by
    both item and account deletion (`Booker/Services/ItemManager.cs:355`,
    `Booker/Services/UserPhotoManager.cs:26`).
  - `DeletePhotosAsync(keys)` — deletes objects, returns the keys it failed
    to delete (orphan list; read the method before asserting exact failure
    semantics — see Step 3).
- `Booker/Pages/Shared/ImageUploadValidation.cs` — static
  `ValidateAndReadAsync(List<IFormFile>?, requireAtLeastOne, ModelStateDictionary, modelKey)`:
  rejects empty batches when required, > `MaxImageCount` (6), non-`image/*`
  content types, empty files, > `MaxImageSizeBytes` (5 MB, from
  `ItemInputModel.cs:7-13`), extensions outside `.jpg/.jpeg/.png`, and bytes
  failing `ImageFormatDetector` (returns the detected extension, not the
  claimed one). Returns `ValidatedImageBatch(streams, extensions)` or `null`.
- `Booker/Services/UserPhotoManager.cs` — `CollectPhotoKeysAsync(user)`
  (lines 16-31): reads item photos + profile photo BEFORE deletion;
  `DeleteFromStorageAsync(userId, keys)` (lines 38-53): storage failure is
  logged, never thrown.
- `Booker/Areas/Admin/Pages/Users.cshtml.cs` — `OnPostDeleteAsync(int id)`
  (lines 58-85): `CollectPhotoKeysAsync` → `UserManager.DeleteAsync` →
  `DeleteFromStorageAsync`; returns `Content("User deleted successfully.")`.
- Fixtures from plan 001: `S3Recorder` (records `PutObjectRequest`s and
  `DeleteObjectsRequest`s, `FailDeletes` switch),
  `CustomWebApplicationFactory`, `TestSeed`.
- JPEG magic bytes for tests: `FF D8 FF` + padding. PNG:
  `89 50 4E 47 0D 0A 1A 0A` + padding. GIF is NOT detected
  (`ImageFormatDetector` supports JPEG/PNG only) and `.gif` is not in
  `AllowedImageExtensions`.

## Commands you will need

| Purpose | Command | Expected on success |
|---|---|---|
| Build | `dotnet build Booker/Booker.sln --nologo` | exit 0 |
| Run this suite | `dotnet test Booker/Booker.sln --nologo --filter "FullyQualifiedName~Photos"` | all pass |
| Run all | `dotnet test Booker/Booker.sln --nologo` | all pass |

## Scope

**In scope** (create only):
- `Booker.Tests/Services/ImageFormatDetectorTests.cs`
- `Booker.Tests/Services/PhotosManagerTests.cs`
- `Booker.Tests/Services/UserPhotoManagerTests.cs`
- `Booker.Tests/Pages/ImageUploadValidationTests.cs`
- `Booker.Tests/Integration/AccountDeletionTests.cs`

**Out of scope**:
- Any production file (incl. adding GIF support — a product decision, not a
  test task).
- `Booker/Areas/Identity/Pages/Account/Manage/ProfilePictureUpload.cshtml.cs`
  beyond what `AccountDeletionTests` touches via HTTP — its page-level
  validation is covered by the shared `ImageUploadValidation` tests.
- E2E crop/upload UX — plan 005.

## Git workflow

- Branch: `feature/tests-photo-pipeline`
- Commit message style: "Add ImageFormatDetector and PhotosManager tests",
  "Cover account deletion photo purge" — short imperative.
- Do NOT push or open a PR unless the operator instructed it.

## Steps

### Step 1: ImageFormatDetector table tests

`ImageFormatDetectorTests` — `[Theory]` over: JPEG header → `.jpg`; full PNG
signature → `.png`; truncated PNG (first 4 bytes only) → `null`; empty
stream → `null`; text bytes (`"hello"` UTF-8) → `null`; stream position is
restored after a successful call (assert `stream.Position == 0` for a
seekable input positioned at 0); detection works mid-stream (position 4 of a
longer buffer → still reads the header from the CURRENT position).

**Verify**: filtered test run green.

### Step 2: PhotosManager unit tests

Build the manager like plan 002's host (NullLogger, `Lazy<IAmazonS3>` over
`S3Recorder.BuildClient()`, in-memory config with `S3:BucketName` +
`CF:PublicUrl`). Cases:

- `AddPhotoAsync` records a `PutObjectRequest` with `CannedACL == PublicRead`,
  key ending `.jpg` (or detected-normalized extension), and
  `ContentType == "image/jpeg"` when fed JPEG bytes labeled `.jpg`.
- PNG bytes labeled `.jpg` → key ends `.jpg` (extension as passed) but
  `ContentType == "image/png"` (magic bytes win — the `8d76895` contract).
- Missing `S3:BucketName` (config without the key) → `PhotoStorageException`.
- `StorageKeys("k1;https://ext/img.png;k2.jpg;/root/asset")` → `["k1", "k2.jpg"]`.
- `StorageKeys(null)` / `StorageKeys("")` → empty.
- `GetPhotoUrl("k1")` → `CF:PublicUrl` + key joined without a double slash
  (pin the exact joining by reading `GetPhotoUrl` first; assert what it does
  today — characterization).
- `DeletePhotosAsync` happy path → one `DeleteObjectsRequest` containing the
  keys; `FailDeletes = true` → returns the keys as orphaned (verify against
  the actual return contract after reading the method body; if failures
  throw instead, adapt the test to the real contract and note it — that IS
  the characterization).

**Verify**: filtered run green.

### Step 3: ImageUploadValidation tests

Direct calls with constructed `FormFile`s (` MemoryStream` + headers via
`new FormFile(stream, 0, length, "Input.Images", name)` — set `ContentType`
and `FileName` explicitly; `FileName` drives the extension check, bytes
drive detection). Cases: none + `requireAtLeastOne: true` → null + model
error; none + false → empty batch; 7 files → error; content type
`application/pdf` → error; zero-length file → error; 6 MB file → error;
`.gif` name → error; text bytes named `fake.jpg` → error ("nie jest
prawidłowym obrazem"); valid JPEG named `a.jpeg` → batch with `.jpg`
extension (detected canonical); valid PNG named `a.png` → `.png`; mixed
valid + invalid → null (any error fails the batch).

**Verify**: filtered run green.

### Step 4: UserPhotoManager ordering + integration purge

Unit (SQLite context like plan 002's host): seed user with `Photo = "p1"`
and two items (`Photo = "i1;i2"` and `Photo = "https://x/y.png"`).
- `CollectPhotoKeysAsync` returns exactly `["i1", "i2", "p1"]` (external URL
  filtered; item keys before profile key).
- `DeleteFromStorageAsync` with `FailDeletes = true` does NOT throw.

Integration (`AccountDeletionTests`, via `CustomWebApplicationFactory` +
`AuthHttpClient.LoginAsync` from plan 003 as admin — copy the helper or move
it to `Booker.Tests/Infrastructure/` and update plan 003 references):

- Seed victim user with profile photo `p1` and an item with photos
  `i1;i2`; seed admin; login as admin; POST
  `/Admin/Users?handler=Delete&id={victimId}` with antiforgery token
  (fetch `/Admin/Users` first, reuse the token regex from plan 003).
- Assert: response success; victim gone from `factory.Services` DataContext;
  `factory.S3.Deletes` contains exactly the three bare keys — the GDPR
  contract of `6ea5f1a`. The DB cascade removing items is what makes
  collecting keys BEFORE `DeleteAsync` load-bearing; if this test ever fails
  with zero deletes, the ordering regressed.

**Verify**: full `dotnet test` green.

## Test plan

This plan IS a test plan; ≥ 20 cases across steps 1-4. No production edits.

## Done criteria

- [ ] `dotnet build Booker/Booker.sln --nologo` exits 0
- [ ] `dotnet test Booker/Booker.sln --nologo` exits 0; ≥ 20 new photo-pipeline cases pass
- [ ] `git status` shows no production file modified
- [ ] `plans/README.md` status row updated

## STOP conditions

- Any "Current state" excerpt mismatches the live code (drift since `32efc8d`).
- `PhotosManager.DeletePhotosAsync`'s failure contract cannot be exercised
  through `S3Recorder.FailDeletes` (e.g. it catches different exception
  types) — read the method; if the recorder needs a materially different
  failure injection, extend `S3Recorder` in the TEST project only and
  report the deviation.
- The admin delete POST cannot be made to succeed (route/handler name
  differs from `Users.cshtml.cs:58`) — verify by reading the page markup
  (`Booker/Areas/Admin/Pages/Users.cshtml` / `_UserRows.cshtml`) before
  concluding drift.

## Maintenance notes

- `StorageKeys` semantics are shared by item deletion and account deletion;
  both test files pin them. A change there should light up exactly one test
  in each — that is the locality working.
- The integration test asserts the R2 objects recorded by the fake — it
  cannot catch bucket/policy misconfigurations on the real account. That
  remains deploy-time risk, deliberately out of scope.
- If `ItemInputModel.AllowedImageExtensions` grows (e.g. webp), the
  detector must grow with it — the Step 1 table will fail loudly, which is
  the intended prompt.
