#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booker.Areas.Admin.Pages;

public static class AdminNavPages
{
    public static string Index => "Index";
    public static string Users => "Users";

    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
    public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);

    public static string PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "page" : null;
    }
}
