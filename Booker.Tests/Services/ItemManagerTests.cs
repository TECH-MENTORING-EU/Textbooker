using System.Net;
using Booker.Data;
using Booker.Services;
using Microsoft.EntityFrameworkCore;

namespace Booker.Tests.Services;

/// <summary>
/// Characterization tests for ItemManager: school isolation (the 680a224 regression),
/// eager loading of User.School (6508a31), list filtering, view tracking and the
/// add/update/delete lifecycle including photo-key handling.
/// </summary>
public class ItemManagerTests : IAsyncDisposable
{
    private readonly ItemManagerTestHost _host = new();

    private ItemManager Items => _host.Items;
    private ItemManagerTestHost Host => _host;

    // Seed layout used by most tests.
    private readonly int _userA;       // school A
    private readonly int _userB;       // school B
    private readonly int _userNo;      // no school
    private readonly int _itemA;
    private readonly int _itemB;
    private readonly int _itemNo;

    public ItemManagerTests()
    {
        _userA = _host.SeedUserAsync("userA", "a.edu.pl").GetAwaiter().GetResult();
        _userB = _host.SeedUserAsync("userB", "b.edu.pl").GetAwaiter().GetResult();
        _userNo = _host.SeedUserAsync("userNo").GetAwaiter().GetResult();
        _itemA = _host.SeedItemAsync(_userA, price: 10m).GetAwaiter().GetResult();
        _itemB = _host.SeedItemAsync(_userB, price: 20m).GetAwaiter().GetResult();
        _itemNo = _host.SeedItemAsync(_userNo, price: 30m).GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    private Task<User> User(int id) => Host.GetUserAsync(id);

    // ---------------------------------------------------------------- isolation

    [Fact]
    public async Task GetItemAsync_anonymous_sees_items_from_all_schools()
    {
        Assert.NotNull(await Items.GetItemAsync(_itemA, null));
        Assert.NotNull(await Items.GetItemAsync(_itemB, null));
        Assert.NotNull(await Items.GetItemAsync(_itemNo, null));
    }

    [Fact]
    public async Task GetItemAsync_user_sees_own_school_item_only()
    {
        var userA = await User(_userA);

        Assert.NotNull(await Items.GetItemAsync(_itemA, userA));          // same school
        Assert.Null(await Items.GetItemAsync(_itemB, userA));             // other school (680a224)
        Assert.Null(await Items.GetItemAsync(_itemNo, userA));            // schoolless owner
    }

    [Fact]
    public async Task GetItemAsync_schoolless_user_sees_schoolless_items_only()
    {
        var userNo = await User(_userNo);

        Assert.NotNull(await Items.GetItemAsync(_itemNo, userNo));
        Assert.Null(await Items.GetItemAsync(_itemA, userNo));
    }

    [Fact]
    public async Task GetItemAsync_unknown_id_returns_null_for_everyone()
    {
        Assert.Null(await Items.GetItemAsync(999999, null));
        Assert.Null(await Items.GetItemAsync(999999, await User(_userA)));
    }

    [Fact]
    public async Task GetItemAsync_admin_overload_returns_item_across_schools_and_loads_school()
    {
        // The id-only overload is the admin path (no school filter); the 6508a31 regression
        // was the missing ThenInclude(u => u.School) - touching it must not NRE.
        var item = await Items.GetItemAsync(_itemB);
        Assert.NotNull(item);
        Assert.Equal(_userB, item.UserId);
        Assert.NotNull(item.User.School);
        Assert.Equal("b.edu.pl", item.User.School!.EmailDomain);
    }

    [Fact]
    public async Task GetAllItemsAsync_and_count_follow_school_isolation()
    {
        var userA = await User(_userA);
        var userNo = await User(_userNo);

        var forAnonymous = await Items.GetAllItemsAsync(null).Materialize();
        var forA = await Items.GetAllItemsAsync(userA).Materialize();
        var forNo = await Items.GetAllItemsAsync(userNo).Materialize();

        Assert.Equal(3, forAnonymous.Count);
        Assert.Single(forA);
        Assert.Equal(_itemA, forA.Single().Id);
        Assert.Single(forNo);
        Assert.Equal(_itemNo, forNo.Single().Id);

        Assert.Equal(3, await Items.GetAllItemsCountAsync(null));
        Assert.Equal(1, await Items.GetAllItemsCountAsync(userA));
        Assert.Equal(1, await Items.GetAllItemsCountAsync(userNo));
    }

    // ---------------------------------------------------------------- filtering

    [Fact]
    public async Task GetItemIdsByParamsAsync_search_matches_title_substring()
    {
        // The filter lowercases the search term but not the title. On SqlServer (production)
        // Contains() maps to a case-insensitive LIKE, so any casing matches; on SQLite
        // (test host) it maps to a case-sensitive instr(), so the contract is exercised
        // with an all-lowercase run of the title - the fragment that matches on both.
        var anyBook = await Host.Context.Books.OrderBy(b => b.Id).FirstAsync();
        var fragment = new string(anyBook.Title.Skip(1).TakeWhile(char.IsLower).ToArray());
        if (fragment.Length < 2)
        {
            return; // catalog shape changed - no lowercase run to search with
        }

        var ids = await Items.GetItemIdsByParamsAsync(
            new ItemManager.Parameters(fragment, [], null, null, null, null), null).Materialize();

        Assert.Contains(_itemA, ids);
    }

    [Fact]
    public async Task GetItemIdsByParamsAsync_price_bounds_are_inclusive()
    {
        var ids = await Items.GetItemIdsByParamsAsync(
            new ItemManager.Parameters(null, [], null, null, MinPrice: 10m, MaxPrice: 20m), null).Materialize();

        Assert.Contains(_itemA, ids); // 10
        Assert.Contains(_itemB, ids); // 20
        Assert.DoesNotContain(_itemNo, ids); // 30
    }

    [Fact]
    public async Task GetItemIdsByParamsAsync_subject_level_and_grade_filters_intersect()
    {
        var book = await Host.Context.Books
            .Include(b => b.Grades)
            .OrderBy(b => b.Id)
            .FirstAsync();

        var ids = await Items.GetItemIdsByParamsAsync(
            new ItemManager.Parameters(null, [.. book.Grades], book.Subject, book.Level, null, null), null).Materialize();

        Assert.Contains(_itemA, ids);

        var withWrongSubject = await Items.GetItemIdsByParamsAsync(
            new ItemManager.Parameters(null, [.. book.Grades], new Subject { Id = 999, Name = "nieistniejący" }, book.Level, null, null), null).Materialize();
        Assert.Empty(withWrongSubject);

        var withWrongLevel = await Items.GetItemIdsByParamsAsync(
            new ItemManager.Parameters(null, [.. book.Grades], book.Subject, new Level { Id = 999, Name = "nieistniejący" }, null, null), null).Materialize();
        Assert.Empty(withWrongLevel);
    }

    [Fact]
    public async Task GetUserItemsCountAsync_is_scoped_to_the_owner()
    {
        await Host.SeedItemAsync(_userA);

        Assert.Equal(2, await Items.GetUserItemsCountAsync(_userA));
        Assert.Equal(1, await Items.GetUserItemsCountAsync(_userB));
    }

    // ---------------------------------------------------------------- view tracking

    [Fact]
    public async Task TrackViewAsync_second_call_is_a_no_op()
    {
        var otherSchoolUser = await User(_userB); // any user that is not the owner

        await Items.TrackViewAsync(_itemA, _userB);
        await Items.TrackViewAsync(_itemA, _userB);

        Assert.Equal(1, await Items.GetViewCountAsync(_itemA));
        Assert.Equal(0, await Items.GetViewCountAsync(_itemB));
    }

    // ---------------------------------------------------------------- lifecycle

    [Fact]
    public async Task MarkItemReservedAsync_round_trips()
    {
        await Items.MarkItemReservedAsync(_itemA, true);
        Assert.True((await Host.Context.Items.FindAsync(_itemA))!.Reserved);

        await Items.MarkItemReservedAsync(_itemA, false);
        Assert.False((await Host.Context.Items.FindAsync(_itemA))!.Reserved);
    }

    private async Task<StaticDataManager.Parameters> ValidParametersForFirstBookAsync()
    {
        var book = await Host.Context.Books
            .Include(b => b.Grades)
            .OrderBy(b => b.Id)
            .FirstAsync();
        var grades = await Host.StaticData.GetGradesByBookTitleAsync(book.Title);
        return new StaticDataManager.Parameters(book.Title, grades, book.Subject, book.Level);
    }

    private ItemManager.ItemModel Model(int ownerId, StaticDataManager.Parameters parameters, string photo) =>
        new(
            Host.GetUserAsync(ownerId).GetAwaiter().GetResult(),
            parameters,
            "opis",
            "dobry",
            15m,
            ImageStreams: null,
            ImageFileExtensions: null,
            ExistingImageFileNames: photo);

    [Fact]
    public async Task AddItemAsync_valid_model_creates_item_and_passes_existing_photo_through()
    {
        var parameters = await ValidParametersForFirstBookAsync();

        var result = await Items.AddItemAsync(Model(_userA, parameters, "key1;key2"));

        Assert.Equal(ItemManager.Status.Success, result.Status);
        var item = await Host.Context.Items.FindAsync(result.Id);
        Assert.NotNull(item);
        Assert.Equal("key1;key2", item.Photo);
        Assert.Equal(15m, item.Price);
    }

    [Fact]
    public async Task AddItemAsync_with_streams_uploads_and_joins_generated_keys()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var model = new ItemManager.ItemModel(
            await User(_userA),
            parameters,
            "opis",
            "dobry",
            15m,
            ImageStreams: [JpegStream(), JpegStream()],
            ImageFileExtensions: [".jpg", ".jpg"],
            ExistingImageFileNames: null);

        var result = await Items.AddItemAsync(model);

        Assert.Equal(ItemManager.Status.Success, result.Status);
        Assert.Equal(2, Host.S3.Puts.Count);
        Assert.All(Host.S3.Puts, put => Assert.Equal("test-bucket", put.BucketName));
        var item = await Host.Context.Items.FindAsync(result.Id);
        Assert.NotNull(item);
        var keys = item.Photo.Split(';', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, keys.Length);
        Assert.All(keys, k => Assert.EndsWith(".jpg", k));
        Assert.Equal(keys, Host.S3.Puts.Select(p => p.Key));
    }

    [Fact]
    public async Task AddItemAsync_payload_mismatch_returns_error_and_uploads_nothing()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var model = new ItemManager.ItemModel(
            await User(_userA),
            parameters,
            "opis",
            "dobry",
            15m,
            ImageStreams: [JpegStream()],
            ImageFileExtensions: [".jpg", ".png"], // 1 stream, 2 extensions
            ExistingImageFileNames: null);

        var result = await Items.AddItemAsync(model);

        Assert.Equal(ItemManager.Status.Error, result.Status);
        Assert.Empty(Host.S3.Puts);
    }

    [Fact]
    public async Task AddItemAsync_unknown_title_returns_invalid_title()
    {
        // InvalidTitle (rather than a bare Error) requires the other parameters to be
        // present: the first guard in ValidateItemModelAsync rejects null subject/level/
        // grades with plain Status.Error before the catalog lookup happens.
        var template = await ValidParametersForFirstBookAsync();
        var parameters = template with { Title = "Tytuł, który nie istnieje w katalogu" };

        var result = await Items.AddItemAsync(Model(_userA, parameters, "key"));

        // InvalidTitle must carry the Error bit, otherwise AddItemAsync treats the result
        // as a success and binds the item to the placeholder "Inna" book (Id -1). This is
        // the bug found while writing this suite; ValidateAndReturn in BookFormModel
        // expects the combined flags to surface its InvalidTitle message.
        Assert.Equal(ItemManager.Status.Error | ItemManager.Status.InvalidTitle, result.Status);
        Assert.Equal(-1, result.Id);
    }

    [Fact]
    public async Task AddItemAsync_wrong_subject_returns_invalid_subject()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var wrong = parameters with { Subject = new Subject { Id = 999, Name = "nieistniejący" } };

        var result = await Items.AddItemAsync(Model(_userA, wrong, "key"));

        Assert.True(result.Status.HasFlag(ItemManager.Status.InvalidSubject));
        Assert.True(result.Status.HasFlag(ItemManager.Status.Error));
    }

    [Fact]
    public async Task AddItemAsync_wrong_level_returns_invalid_level()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var wrong = parameters with { Level = new Level { Id = 999, Name = "nieistniejący" } };

        var result = await Items.AddItemAsync(Model(_userA, wrong, "key"));

        Assert.True(result.Status.HasFlag(ItemManager.Status.InvalidLevel));
        Assert.True(result.Status.HasFlag(ItemManager.Status.Error));
    }

    [Fact]
    public async Task AddItemAsync_grades_in_wrong_order_are_rejected()
    {
        // Characterization: validation compares grades with SequenceEqual, so the order
        // of the submitted list matters. This pins that (surprising) behavior on purpose;
        // if it is ever made order-insensitive, update this test together with the change.
        var parameters = await ValidParametersForFirstBookAsync();
        if (parameters.Grades.Count < 2)
        {
            return; // catalog shape changed - nothing to reorder
        }
        var reordered = parameters with { Grades = [.. parameters.Grades.AsEnumerable().Reverse()] };

        var result = await Items.AddItemAsync(Model(_userA, reordered, "key"));

        Assert.True(result.Status.HasFlag(ItemManager.Status.InvalidGrades));
        Assert.True(result.Status.HasFlag(ItemManager.Status.Error));
    }

    [Fact]
    public async Task UpdateItemAsync_new_streams_replace_the_whole_photo_list()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var item = (await Host.Context.Items.FindAsync(_itemA))!;
        var model = new ItemManager.ItemModel(
            await User(_userA),
            parameters,
            "nowy opis",
            "jak nowy",
            99m,
            ImageStreams: [JpegStream()],
            ImageFileExtensions: [".jpg"],
            ExistingImageFileNames: "old-key");

        var status = await Items.UpdateItemAsync(item, model);

        Assert.Equal(ItemManager.Status.Success, status);
        await Host.Context.Entry(item).ReloadAsync();
        Assert.Equal(99m, item.Price);
        Assert.Single(Host.S3.Puts);
        Assert.Equal(Host.S3.Puts[0].Key, item.Photo); // old key gone, replaced by the new one
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public async Task UpdateItemAsync_without_streams_keeps_existing_photo()
    {
        var parameters = await ValidParametersForFirstBookAsync();
        var item = (await Host.Context.Items.FindAsync(_itemA))!;
        item.Photo = "kept-key";
        await Host.Context.SaveChangesAsync();
        var model = Model(_userA, parameters, "kept-key");

        var status = await Items.UpdateItemAsync(item, model);

        Assert.Equal(ItemManager.Status.Success, status);
        Assert.Empty(Host.S3.Puts);
        await Host.Context.Entry(item).ReloadAsync();
        Assert.Equal("kept-key", item.Photo);
    }

    [Fact]
    public async Task DeleteItemAsync_removes_row_and_deletes_only_bare_storage_keys()
    {
        var itemId = await Host.SeedItemAsync(
            _userA, photo: "key1;https://external.example/img.png;/root-relative.png;key2.jpg");

        await Items.DeleteItemAsync(itemId);

        Assert.Null(await Host.Context.Items.FindAsync(itemId));
        Assert.Equal(["key1", "key2.jpg"], Host.S3.Deletes.Select(d => d.Key));
    }

    [Fact]
    public async Task SetItemsVisibilityByUserAsync_hides_only_that_owner()
    {
        await Items.SetItemsVisibilityByUserAsync(_userA, false);

        Assert.False((await Host.Context.Items.FindAsync(_itemA))!.IsVisible);
        Assert.True((await Host.Context.Items.FindAsync(_itemB))!.IsVisible);
    }

    private static MemoryStream JpegStream()
    {
        var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46]);
        stream.Position = 0;
        return stream;
    }
}
