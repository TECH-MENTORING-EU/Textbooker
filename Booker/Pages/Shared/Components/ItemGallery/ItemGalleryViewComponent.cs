using Booker.Services;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;

namespace Booker.Pages.Shared.Components.ItemGallery;

public class ItemGalleryViewComponent : ViewComponent
{
    private readonly ItemManager _itemManager;
    private readonly PhotosManager _photosManager;
    private readonly UserManager<User> _userManager;
    const int PageSize = 25;

    public record ItemsListModel(
        IEnumerable<ItemModel> Items,
        StaticDataManager.Parameters Params,
        int PageNumber,
        bool HasMorePages
    );

    public record ItemModel(
        Item Item,
        string FirstPhoto,
        StaticDataManager.Parameters Params,
        bool LinkFilters
    );

    public ItemGalleryViewComponent(ItemManager itemManager, UserManager<User> userManager, PhotosManager photosManager)
    {
        _itemManager = itemManager;
        _userManager = userManager;
        _photosManager = photosManager;
    }

    /// <summary>
    /// Renders one page of the listing. Pass <paramref name="itemFilters"/> to page a
    /// filtered query entirely in SQL (Browse); pass <paramref name="itemIds"/> to page a
    /// concrete id set (favorites, profiles, landing page).
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync(
        StaticDataManager.Parameters parameters,
        IEnumerable<int>? itemIds = null,
        ItemManager.Parameters? itemFilters = null,
        int pageNumber = 0,
        int pageSize = PageSize,
        bool showHidden = false,
        bool linkFilters = false
    )
    {
        var ids = itemIds?.ToList();

        if (itemFilters is null && ids is null or { Count: 0 })
        {
            return new HtmlContentViewComponentResult(
                new HtmlString("<p>Brak wyników...</p>")
            );
        }

        var currentUser = UserClaimsPrincipal.Identity?.IsAuthenticated == true
            ? await _userManager.GetUserAsync(UserClaimsPrincipal)
            : null;

        var page = itemFilters is not null
            ? await _itemManager.GetPagedItemsByParamsAsync(itemFilters, pageNumber, pageSize, currentUser, includeHidden: showHidden)
            : await _itemManager.GetPagedItemsByIdsAsync(ids!, pageNumber, pageSize, currentUser, includeHidden: showHidden);

        var itemsWithPhotos = page.Items.Select(item => new ItemModel(
            Item: item,
            FirstPhoto: string.IsNullOrEmpty(item.Photo)
                ? "/img/default-book.svg"
                : _photosManager.GetPhotoUrl(item.Photo.Split(';')[0].Trim()),
            Params: parameters,
            LinkFilters: linkFilters
        ));

        return View(
            new ItemsListModel(
                Items: itemsWithPhotos,
                Params: parameters,
                PageNumber: pageNumber,
                HasMorePages: page.HasMorePages
            ));
    }
}
