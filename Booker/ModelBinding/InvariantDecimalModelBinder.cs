using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Booker.ModelBinding;

/// <summary>
/// Binds <see cref="decimal"/> and <see cref="decimal"/>? model fields (prices and price filters)
/// independently of the thread culture. The default <c>SimpleTypeModelBinder</c> follows the value
/// provider's culture, which differs per source in ASP.NET Core: query strings bind with the
/// invariant culture while form fields bind with the current culture (pl-PL here), so the same
/// keystrokes were read differently per page - "12,50" silently became 1250 in the Browse price
/// filters, "12.50" was rejected by the Add/Edit forms. This binder accepts "12.50" and "12,50"
/// as twelve and a half on every surface, and rejects values whose meaning would depend on group
/// separators ("1.234.567", "1,234.56") with a validation error instead of silently reinterpreting them.
/// </summary>
public class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var attemptedValue = valueProviderResult.FirstValue?.Trim();
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        if (string.IsNullOrEmpty(attemptedValue))
        {
            // Leave the model unset so [Required] (or a null nullable) produces the field's own message.
            return Task.CompletedTask;
        }

        if (TryParseInvariantDecimal(attemptedValue, out var parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                bindingContext.ModelMetadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(
                    attemptedValue, bindingContext.ModelMetadata.GetDisplayName()));
        }

        return Task.CompletedTask;
    }

    private static bool TryParseInvariantDecimal(string value, out decimal result)
    {
        result = 0;
        if (value.Count(c => c is '.' or ',') > 1)
        {
            return false;
        }

        return decimal.TryParse(
            value.Replace(',', '.'),
            NumberStyles.Number & ~NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out result);
    }
}

/// <summary>
/// Replaces the default <c>SimpleTypeModelBinder</c> for <see cref="decimal"/> and
/// <see cref="decimal"/>? with <see cref="InvariantDecimalModelBinder"/>. Every decimal bound
/// from HTTP in this app is a money amount (item price, browse price filters), so the
/// replacement is scoped to that type only.
/// </summary>
public class InvariantDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        if (modelType == typeof(decimal) || modelType == typeof(decimal?))
        {
            return new InvariantDecimalModelBinder();
        }

        return null;
    }
}
