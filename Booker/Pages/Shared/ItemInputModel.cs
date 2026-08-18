using System;
using System.ComponentModel.DataAnnotations;

namespace Booker.Pages.Shared;

public abstract class ItemInputModel
{
    public const int MaxDescriptionLength = 200;
    public const int MaxStateLength = 40;
    public const int MaxImageCount = 6;
    public const long MaxImageSizeBytes = 5 * 1024 * 1024;
    public const int MaxImageSizeMb = 5;
    public static readonly HashSet<string> AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png"
    ];

    [Required(ErrorMessage = "Proszę wybrać tytuł książki.")]
    public required string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać przedmiot.")]
    public required string Subject { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać klasę.")]
    public required string Grade { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać poziom.")]
    public required string Level { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę podać opis ogłoszenia.")]
    [StringLength(MaxDescriptionLength, ErrorMessage = "Opis ogłoszenia nie może przekraczać {1} znaków.")]
    public required string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę opisać stan książki.")]
    [StringLength(MaxStateLength, ErrorMessage = "Opis stanu książki nie może przekraczać {1} znaków.")]
    public required string State { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę podać cenę.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od zera.")]
    public required decimal Price { get; set; } = 0;

    [Display(Name = "Zdjęcia książki")]
    public virtual List<IFormFile> Images { get; set; } = new();
}

public class ItemAddModel : ItemInputModel
{
}

public class ItemEditModel : ItemInputModel
{
    [Display(Name = "Zarezerwowane")]
    public bool Reserved { get; set; }
}