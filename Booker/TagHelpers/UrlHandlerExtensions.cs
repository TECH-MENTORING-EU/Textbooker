using Faker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Booker.TagHelpers;

public static class UrlHandlerExtensions
{
    public static string? Handler(this IUrlHelper helper, string handler, object? values = null)
    {
        ArgumentNullException.ThrowIfNull(helper);

        // Convert the values object to a dictionary
        var routeValues = new RouteValueDictionary(values)
        {
            ["handler"] = handler // Add the handler to the dictionary
        };

        return helper.RouteUrl(values);
    }
}