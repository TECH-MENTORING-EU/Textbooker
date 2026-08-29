using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Booker.Areas.Admin.Pages
{
    // RODO — zadanie 08: minimalna lista ogłoszeń z oznaczeniem tych do przejrzenia
    // (opis wygląda na zawierający dane kontaktowe). Bez pełnej kolejki moderacyjnej.
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
