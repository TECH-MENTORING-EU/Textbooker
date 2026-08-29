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

        public ItemsModel(ItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        public List<Item> Items { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            Items = await _itemManager.GetAllItemsAsync(null).ToListAsync();
            return Page();
        }
    }
}
