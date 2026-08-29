// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Booker.Data;
using Booker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Booker.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ItemManager _itemManager;
        private readonly FavoritesManager _favoritesManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(
            UserManager<User> userManager,
            ItemManager itemManager,
            FavoritesManager favoritesManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _itemManager = itemManager;
            _favoritesManager = favoritesManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nie znaleziono użytkownika o ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("Użytkownik z ID '{UserId}' pobrał swoje dane osobowe.", _userManager.GetUserId(User));

            var account = new Dictionary<string, string>();
            var personalDataProps = typeof(User).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                account.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                account.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            // RODO: the 2FA authenticator key is deliberately excluded - it's a live secret, and
            // this export file ends up on disks and in inboxes where a leaked key is exploitable
            // (unlike the rest of the export, which is inert record-keeping).

            var itemIds = await _itemManager.GetUserItemIdsAsync(user.Id).ToListAsync();
            var items = new List<object>();
            await foreach (var item in _itemManager.GetItemsByIdsAsync(itemIds, user))
            {
                items.Add(new
                {
                    item.Id,
                    Tytul = item.Book.Title,
                    Przedmiot = item.Book.Subject.Name,
                    Klasy = item.Book.Grades.Select(g => g.GradeNumber),
                    Poziom = item.Book.Level.Name,
                    item.Description,
                    item.State,
                    item.Price,
                    item.CreatedAt,
                    item.UpdatedAt,
                    item.Reserved,
                    item.IsVisible,
                    Wyswietlenia = await _itemManager.GetViewCountAsync(item.Id),
                    ZdjeciaUrl = _itemManager.GetPhotosUrl(item)
                });
            }

            var favoriteIds = await _favoritesManager.GetFavoriteIdsAsync(user.Id);
            var favorites = new List<object>();
            await foreach (var item in _itemManager.GetItemsByIdsAsync(favoriteIds, user))
            {
                favorites.Add(new
                {
                    item.Id,
                    Tytul = item.Book.Title,
                    Sprzedawca = item.User.UserName,
                    item.Price
                });
            }

            var export = new
            {
                konto = account,
                ogloszenia = items,
                ulubione = favorites
            };

            var json = JsonSerializer.SerializeToUtf8Bytes(export, new JsonSerializerOptions { WriteIndented = true });

            var fileName = $"PersonalData_{user.Id}_{DateTime.UtcNow:yyyyMMdd}.json";
            Response.Headers.TryAdd("Content-Disposition", $"attachment; filename={fileName}");
            return new FileContentResult(json, "application/json");
        }
    }
}
