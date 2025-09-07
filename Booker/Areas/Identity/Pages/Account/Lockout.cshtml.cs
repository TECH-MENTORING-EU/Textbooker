// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Booker.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        public bool HasDateTime { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool IsLongLockout { get; set; }
        public bool IsForever { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public void OnGet(long? lockoutEnd)
        {
            HasDateTime = lockoutEnd.HasValue;
            LockoutEnd = DateTimeOffset.FromUnixTimeSeconds(lockoutEnd ?? 0).LocalDateTime;
            IsLongLockout = LockoutEnd.Value > DateTime.Now.AddMinutes(5);
            IsForever = LockoutEnd.Value > DateTime.Now.AddYears(100);
        }
    }
}
