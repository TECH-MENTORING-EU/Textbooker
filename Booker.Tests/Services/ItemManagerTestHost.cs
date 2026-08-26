using Amazon;
using Amazon.S3;
using Booker.Data;
using Booker.Services;
using Booker.Tests.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Booker.Tests.Services;

/// <summary>
/// Owns one SQLite in-memory database and the real manager graph over it:
/// ItemManager + StaticDataManager + PhotosManager backed by the NSubstitute S3 double.
/// No web host involved - these are characterization tests of the service layer.
/// </summary>
public sealed class ItemManagerTestHost : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public DataContext Context { get; }
    public S3Recorder S3 { get; } = new();
    public StaticDataManager StaticData { get; }
    public ItemManager Items { get; }

    public ItemManagerTestHost()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new DataContext(options);
        Context.Database.EnsureCreated(); // applies the HasData book catalog

        StaticData = new StaticDataManager(Context, new MemoryCache(Options.Create(new MemoryCacheOptions())));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["S3:BucketName"] = "test-bucket",
                ["CF:PublicUrl"] = "https://cdn.test",
            })
            .Build();
        var photos = new PhotosManager(
            NullLogger<PhotosManager>.Instance,
            new Lazy<IAmazonS3>(S3.BuildClient),
            config);

        Items = new ItemManager(Context, StaticData, photos, NullLogger<ItemManager>.Instance);
    }

    /// <summary>Seeds a user (optionally bound to a new school) and returns its id.</summary>
    public async Task<int> SeedUserAsync(string name, string? schoolDomain = null)
    {
        School? school = null;
        if (schoolDomain != null)
        {
            school = new School { Name = $"school of {name}", EmailDomain = schoolDomain, IsActive = true };
            Context.Schools.Add(school);
            await Context.SaveChangesAsync();
        }

        var user = new User { UserName = name, Email = $"{name}@{schoolDomain ?? "none.example"}", SchoolId = school?.Id };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        return user.Id;
    }

    /// <summary>Seeds an item bound to the first catalog book; photo list defaults to empty.</summary>
    public async Task<int> SeedItemAsync(int ownerId, decimal price = 20m, bool isVisible = true, string photo = "")
    {
        var book = await Context.Books.OrderBy(b => b.Id).FirstAsync();
        var owner = (await Context.Users.FindAsync(ownerId))!;
        var item = new Item
        {
            BookId = book.Id,
            UserId = ownerId,
            Book = book,
            User = owner,
            Description = "seed item",
            State = "dobry",
            Price = price,
            IsVisible = isVisible,
            CreatedAt = DateTime.Now,
            Photo = photo,
        };
        Context.Items.Add(item);
        await Context.SaveChangesAsync();
        return item.Id;
    }

    public Task<User> GetUserAsync(int id) => Context.Users.Where(u => u.Id == id).SingleAsync();

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        _connection.Dispose();
    }
}
