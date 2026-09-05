using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Booker.Utilities;

namespace Booker.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly DataContext _context;
        const int PageSize = 25;

        public IndexModel(ILogger<IndexModel> logger, UserManager<User> userManager, ItemManager itemManager, DataContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _itemManager = itemManager;
            _context = context;
        }
        [FromRoute]
        public int? Id { get; set; }

        public List<int>? ItemIds { get; set; }
        public StaticDataManager.Parameters Params { get; set; } = null!;

        // RODO - task 06: school name fetched separately, because UserManager doesn't load
        // the User.School navigation.
        public record UserModel(User RequestUser, bool IsCurrentUser, string? SchoolName, bool HasActiveListing);
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

            if (user == null || !user.IsVisible)
            {
                return NotFound();
            }

            ItemIds = await _itemManager.GetUserItemIdsAsync(Id.Value).ToListAsync();

            Params = new StaticDataManager.Parameters(null, [], null, null);

            var schoolName = user.SchoolId.HasValue
                ? (await _context.Schools.FindAsync(user.SchoolId.Value))?.Name
                : null;
            var requestingUser = await _userManager.GetUserAsync(User);
            var hasActiveListing = await _itemManager.HasVisibleListingAsync(Id.Value, requestingUser);

            UserInfo = new UserModel(user, user.Id == currentUserId, schoolName, hasActiveListing);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return ViewComponent("ItemGalleryViewComponent", new
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