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
        StaticDataManager.Parameters Params
    );

    public ItemGalleryViewComponent(ItemManager itemManager, UserManager<User> userManager)
    {
        _itemManager = itemManager;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        IEnumerable<int> itemIds,
        StaticDataManager.Parameters parameters,
        int pageNumber = 0,
        int pageSize = PageSize,
        bool showHidden = false
    )
    {
        if (!itemIds.Any())
        {
            return new HtmlContentViewComponentResult(
                new HtmlString("<p>Brak wyników...</p>")
            );
        }

        var currentUser = UserClaimsPrincipal.Identity?.IsAuthenticated == true 
            ? await _userManager.GetUserAsync(UserClaimsPrincipal) 
            : null;

        var query = _itemManager.GetPagedItemsByIdsAsync(itemIds, pageNumber, pageSize, currentUser);
        if (!showHidden)
        {
            query = query.Where(i => i.IsVisible);
        }
        var itemsFromDb = await query.ToListAsync();

        var itemsWithPhotos = itemsFromDb.Select(item => new ItemModel(
            Item: item,
            FirstPhoto: string.IsNullOrEmpty(item.Photo)
                ? "/images/default-book.png" // fallback
                : item.Photo.Split(';')[0].Trim(),
            Params: parameters
        ));

        return View(
            new ItemsListModel(
                Items: itemsWithPhotos,
                Params: parameters,
                PageNumber: pageNumber,
                HasMorePages: itemIds.Count() > (pageNumber + 1) * pageSize
            ));
    }
}
