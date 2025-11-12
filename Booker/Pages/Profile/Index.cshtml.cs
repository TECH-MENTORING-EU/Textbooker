using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.Utilities;
using Microsoft.AspNetCore.Authorization;
using Booker.Authorization;

namespace Booker.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly IAuthorizationService _authorizationService;

        const int PageSize = 25;

        public IndexModel(UserManager<User> userManager, ItemManager itemManager, IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _itemManager = itemManager;
            _authorizationService = authorizationService;
        }
        [FromRoute]
        public int? Id { get; set; }

        public List<int>? ItemIds { get; set; }
        public StaticDataManager.Parameters Params { get; set; } = null!;

        public record UserModel(User RequestUser, bool IsCurrentUser, bool CanView, bool CanViewFavorites);
        public UserModel UserInfo { get; set; } = null!;
        public async Task<IActionResult> OnGetAsync(int pageNumber)
        {
            if (!Id.HasValue)
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Challenge();
                }
                Id = _userManager.GetUserId(User).IntOrDefault();
            }

            var user = await _userManager.FindByIdAsync(Id.Value.ToString());

            var viewProfile = await _authorizationService.AuthorizeAsync(User, user, UserOperations.Read);
            var viewFavorites = await _authorizationService.AuthorizeAsync(User, user, UserOperations.ReadFavorites);
            
            if (user == null || !viewProfile.Succeeded)
            {
                return NotFound();
            }

            ItemIds = await _itemManager.GetUserItemIdsAsync(Id.Value).ToListAsync();

            Params = new StaticDataManager.Parameters(null, [], null, null);

            UserInfo = new UserModel(
                user,
                user.Id == _userManager.GetUserId(User).IntOrDefault(),
                viewProfile.Succeeded,
                viewFavorites.Succeeded
            );

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGallery", new
                {
                    itemIds = ItemIds,
                    parameters = Params,
                    pageNumber = pageNumber,
                    showHidden = UserInfo.IsCurrentUser,
                });
            }
            return Page();
        }
    }
}