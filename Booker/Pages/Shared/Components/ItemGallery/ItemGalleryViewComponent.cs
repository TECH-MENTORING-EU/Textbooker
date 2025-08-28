using Booker.Services;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Html;

namespace Booker.Pages.Shared.Components.ItemGallery;

public class ItemGalleryViewComponent : ViewComponent
{
    private readonly ItemManager _itemManager;
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

    public ItemGalleryViewComponent(ItemManager itemManager)
    {
        _itemManager = itemManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        IEnumerable<int> itemIds,
        StaticDataManager.Parameters parameters,
        int pageNumber = 0,
        int pageSize = PageSize
    )
    {
        if (!itemIds.Any())
        {
            return new HtmlContentViewComponentResult(
                new HtmlString("<p>Brak wyników...</p>")
            );
        }

        var itemsFromDb = await _itemManager.GetPagedItemsByIdsAsync(itemIds, pageNumber, pageSize).ToListAsync();

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
