using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Booker.Data
{
    public class User : IdentityUser<int>
    {
        [PersonalData]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [PersonalData]
        public DateTime? LastActiveAt { get; set; }
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Foreign key to the School table. Nullable - users without assigned school have null.
        /// </summary>
        [PersonalData]
        public int? SchoolId { get; set; }

        /// <summary>
        /// Navigation property to the School entity
        /// </summary>
        public School? School { get; set; }

        [PersonalData]
        public string? Photo { get; set; }
        public ICollection<Item> Items { get; } = new HashSet<Item>();
        public ICollection<Item> Favorites { get; } = new HashSet<Item>();
        public ICollection<ItemView> ItemViews { get; } = new HashSet<ItemView>();

        [PersonalData]
        public bool AreFavoritesPublic { get; set; } = false;

        // RODO — zadanie 05: wartości domyślne widoczności danych kontaktowych.
        // E-mail jest ujawniany na podstawie wykonania umowy (art. 6 ust. 1 lit. b RODO) — stąd domyślnie
        // widoczny. Wszystkie pozostałe kanały są opcjonalne i oparte na zgodzie (art. 6 ust. 1 lit. a) —
        // muszą startować wyłączone.
        [PersonalData]
        public bool DisplayEmail { get; set; } = true;

        [PersonalData]
        public bool DisplayPhone { get; set; } = false;

        [PersonalData]
        public bool DisplayWhatsapp { get; set; } = false;

        [PersonalData]
        public string? FbMessenger { get; set; }

        [PersonalData]
        public bool DisplayMessenger { get; set; } = false;

        [PersonalData]
        public string? Instagram { get; set; }

        [PersonalData]
        public bool DisplayInstagram { get; set; } = false;

        // RODO — zadanie 06
        [PersonalData]
        public bool DisplaySchool { get; set; } = false;

        // RODO — zadanie 04
        [PersonalData]
        public DateTime? TermsAcceptedAt { get; set; }

        [PersonalData]
        public string? TermsAcceptedVersion { get; set; }

        [PersonalData]
        public DateTime? AgeConfirmationAcceptedAt { get; set; }
    }
}
