using Azure.Storage.Blobs.Models;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Booker.Utilities;
using static Booker.Pages.IndexModel;

namespace Booker.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly ItemManager _itemManager;
        const int PageSize = 25;

        public IndexModel(ILogger<IndexModel> logger, UserManager<User> userManager, FavoritesManager favoritesManager, ItemManager itemManager)
        {
            _logger = logger;
            _userManager = userManager;
            _favoritesManager = favoritesManager;
            _itemManager = itemManager;
        }
        [FromRoute]
        public int? Id { get; set; }

        public List<int>? ItemIds { get; set; }
        public StaticDataManager.Parameters Params { get; set; } = null!;

        public record UserModel(User RequestUser, bool IsCurrentUser);
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

            var itemsIds = (await _itemManager.GetUserItemsAsync(Id.Value)).Select(i => i.Id).ToList();

            Params = new StaticDataManager.Parameters(null, null, null, null);

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
    }
}