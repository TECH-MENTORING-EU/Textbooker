using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    // RODO - task 08: a minimal listing of items flagging the ones that need review
    // (description looks like it contains contact details). No full moderation queue.
    public class ItemsModel : PageModel
    {
        private readonly ItemManager _itemManager;

        public const int PageSize = 50;

        public ItemsModel(ItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        public List<ItemManager.AdminItemSummary> Items { get; set; } = [];

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        [FromQuery]
        public int PageNumber { get; set; } = 0;

        [FromQuery]
        public bool OnlyFlagged { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (PageNumber < 0)
            {
                PageNumber = 0;
            }

            TotalCount = await _itemManager.GetAdminItemsCountAsync(OnlyFlagged);
            Items = await _itemManager.GetAdminItemsPageAsync(PageNumber, PageSize, OnlyFlagged);
            return Page();
        }
    }
}
