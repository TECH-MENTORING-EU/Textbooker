using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Booker.Data
{
    public class User : IdentityUser<int>
    {
        public required string School { get; set; }
        public string? Photo {  get; set; }
        public ICollection<Item> Items { get; } = new HashSet<Item>();
    }
}
