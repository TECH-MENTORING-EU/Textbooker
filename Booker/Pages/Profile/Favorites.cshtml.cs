using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Booker.Data;
using Booker.Services;
using Booker.Utilities;
using Microsoft.AspNetCore.Identity;

using static Booker.Pages.Profile.IndexModel;

namespace Booker.Pages.Profile
{
    public class FavoritesModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly FavoritesManager _favoritesManager;
        public FavoritesModel(UserManager<User> userManager, FavoritesManager favoritesManager)
        {
            _userManager = userManager;
            _favoritesManager = favoritesManager;
        }

        public record ButtonState(int Id, bool IsFavorite, bool FullSize);
        [FromRoute]
        public int? Id { get; set; }
        public List<int>? ItemIds { get; set; }
        public StaticDataManager.Parameters Params { get; set; } = null!;
        public UserModel UserInfo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            var currentUserId = _userManager.GetUserId(User).IntOrDefault();

            if (!Id.HasValue)
            {
                if (currentUserId == 0)
                {
                    return Redirect("/Identity/Account/Login");
                }

                Id = currentUserId;
            }

            var user = await _userManager.FindByIdAsync(Id.Value.ToString());

            if (user == null)
            {
                return NotFound();
            }

            Params = new StaticDataManager.Parameters(null, null, null, null);

            ItemIds = await _favoritesManager.GetFavoriteIdsAsync(Id.Value);

            UserInfo = new UserModel(user, user.Id == currentUserId);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGalleryViewComponent", new
                {
                    itemIds = ItemIds,
                    parameters = Params,
                    pageNumber = pageNumber
                });
            }
            return Page();
        }

        public Task<IActionResult> OnPostAddAsync(int itemId, bool fullSize)
            => HandleFavoriteAsync(itemId, fullSize, _favoritesManager.AddFavoriteAsync, true);
        
        public Task<IActionResult> OnPostRemoveAsync(int itemId, bool fullSize)
            => HandleFavoriteAsync(itemId, fullSize, _favoritesManager.RemoveFavoriteAsync, false);
        
        private async Task<IActionResult> HandleFavoriteAsync(
            int itemId, 
            bool fullSize, 
            Func<int, int, Task<FavoritesManager.Status>> action, 
            bool isAdding)
        {
            if (Id != null)
            {
                return BadRequest();
            }
        
            var userId = _userManager.GetUserId(User).IntOrDefault();
        
            if (userId == -1)
            {
                Response.Headers["HX-Redirect"] = Url.Page("/Account/Login", new { area = "Identity" });
                return StatusCode(403); // Forbidden, but will redirect via HTMX
            }
        
            var status = await action(userId, itemId);
        
            switch (status)
            {
                case FavoritesManager.Status.NotFound:
                    return NotFound();
                case FavoritesManager.Status.Forbidden:
                    return Forbid();
                case FavoritesManager.Status.NotModified:
                    return StatusCode(204);
                case FavoritesManager.Status.Success:
                    return Partial("_FavoriteButton", new ButtonState(itemId, isAdding, fullSize));
            }
            return BadRequest();
        }
    }
}
