using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Booker.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Booker.Pages.IndexModel;
using static Booker.Pages.Profile.IndexModel;

namespace Booker.Pages.Profile
{
    public class FavoritesModel : PageModel
    {
        private readonly DataContext _context;
        const int PageSize = 25;
        public FavoritesModel(DataContext context)
        {
            _context = context;
        }

        public record ButtonState(int Id, bool IsFavorite, bool FullSize);
        [FromRoute]
        public int? Id { get; set; }
        public PagedListViewModel? ItemsList { get; set; }
        public FilterParameters? Params { get; set; }
        public UserModel UserInfo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(currentUserIdString, out int currentUserId))
            {
                currentUserId = 0; // Default to 0 if parsing fails
            }

            if (!Id.HasValue)
            {
                if (currentUserId == 0)
                {
                    return Redirect("/Identity/Account/Login");
                }

                Id = currentUserId;
            }

            var user = await _context.Users
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null)
            {
                return NotFound();
            }

            var query = _context.Users
                .Where(u => u.Id == Id)
                .SelectMany(u => u.Favorites)
                .Include(i => i.Book).ThenInclude(b => b.Grades)
                .Include(i => i.Book).ThenInclude(b => b.Subject)
                .Include(i => i.User)
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

        public async Task<IActionResult> OnPostAddAsync(int itemId, bool fullSize)
        {
            if (Id != null)
            {
                return BadRequest();
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return StatusCode(403); // Forbidden, but will redirect via HTMX
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Forbid();
            }

            var item = await _context.Items.FindAsync(itemId);

            if (item == null)
            {
                return NotFound();
            }

            if (user.Favorites.Any(f => f.Id == itemId))
            {
                return StatusCode(204);
            }

            user.Favorites.Add(item);
            await _context.SaveChangesAsync();
            return Partial("_FavoriteButton", new ButtonState(itemId, true, fullSize));
        }
        public async Task<IActionResult> OnPostRemoveAsync(int itemId, bool fullSize)
        {
            if (Id != null)
            {
                return BadRequest();
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return StatusCode(403); // Forbidden, but will redirect via HTMX
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Forbid();
            }

            var item = await _context.Items.FindAsync(itemId);

            if (item == null)
            {
                return NotFound();
            }

            if (!user.Favorites.Any(f => f.Id == itemId))
            {
                return StatusCode(204);
            }

            user.Favorites.Remove(item);
            await _context.SaveChangesAsync();
            return Partial("_FavoriteButton", new ButtonState(itemId, false, fullSize));
        }
    }
}
