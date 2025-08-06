using System;
using System.ComponentModel.DataAnnotations;

namespace Booker.Pages.Shared;

public abstract class ItemInputModel
{
    [Required(ErrorMessage = "Proszę wybrać tytuł książki.")]
    public required string Title { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać przedmiot.")]
    public required string Subject { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać klasę.")]
    public required string Grade { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę wybrać poziom.")]
    public required string Level { get; set; } = string.Empty;
    public required string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę opisać stan książki.")]
    [StringLength(40, ErrorMessage = "Opis stanu książki nie może przekraczać 40 znaków.")]
    public required string State { get; set; } = string.Empty;
    [Required(ErrorMessage = "Proszę podać cenę.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cena musi być większa od zera.")]
    public required decimal Price { get; set; } = 0;

    //[FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Dozwolone są tylko pliki graficzne (jpg, jpeg, png, gif).")]
    //[Length(0, 5 * 1024 * 1024, ErrorMessage = "Plik nie może przekraczać 5 MB.")]
    [Display(Name = "Zdjęcie książki")]
    public virtual IFormFile? Image { get; set; }
}

public class ItemAddModel : ItemInputModel
{
    [Required(ErrorMessage = "Proszę przesłać zdjęcie książki.")]
    public required override IFormFile? Image { get; set; }
}

public class ItemEditModel : ItemInputModel
{

}

