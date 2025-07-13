using System;
using Booker.Services;
using Booker.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Booker.Pages.Shared.Components.ItemGallery;

public class ItemGalleryViewComponent : ViewComponent
{
    private readonly ItemManager _itemManager;
    const int PageSize = 25;

    public record ItemsListModel(
        IEnumerable<Item> Items,
        StaticDataManager.Parameters Params,
        int PageNumber,
        bool HasMorePages
    );

    public record ItemModel(
        Item Item,
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
            return Content("<p>Brak wyników...</p>");
        }

        var itemsFromDb = await _itemManager.GetPagedItemsByIdsAsync(itemIds, pageNumber, pageSize).ToListAsync();


        return View(
            new ItemsListModel(
                itemsFromDb,
                parameters,
                pageNumber,
                itemIds.Count() > (pageNumber + 1) * pageSize
            ));
    }
}
