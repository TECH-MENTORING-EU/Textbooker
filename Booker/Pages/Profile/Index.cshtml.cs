using Azure.Storage.Blobs.Models;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using static Booker.Pages.IndexModel;

namespace Booker.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;
        const int PageSize = 25;

        public IndexModel(ILogger<IndexModel> logger, DataContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public PagedListViewModel? ItemsList { get; set; }
        public FilterParameters? Params { get; set; }
        public record UserModel(User RequestUser, bool IsCurrentUser);
        public UserModel UserInfo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int? id, int pageNumber)
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(currentUserIdString, out int currentUserId))
            {
                currentUserId = 0; // Default to 0 if parsing fails
            }

            if (!id.HasValue)
            {
                if (currentUserId == 0)
                {
                    return Redirect("/Identity/Account/Login");
                }

                id = currentUserId;
            }            

            var user = await _context.Users
                .Include(u => u.Items)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var query = _context.Items
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User).Where(i => i.UserId == id.Value)
                .AsQueryable();

            Params = new FilterParameters(null, null, null, pageNumber);

            var totalItems = await query.CountAsync();
            bool hasMorePages = totalItems > (pageNumber + 1) * PageSize;

            var userFavorites = await _context.Users
            .Where(u => u.Id == currentUserId)
            .SelectMany(u => u.Favorites.Select(f => f.Id))
            .ToListAsync();

            var items = await query
                .OrderByDescending(i => i.DateTime)
                .Skip(pageNumber * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var items2 = items.Select(i => new ItemModel
            (
                i,
                userFavorites.Contains(i.Id),
                Params
            )).ToList();

            ItemsList = new PagedListViewModel(items2, Params, hasMorePages);
            UserInfo = new UserModel(user, user.Id == currentUserId);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return Partial("_ItemGallery", ItemsList);
            }
            return Page();
        }
    }
}
