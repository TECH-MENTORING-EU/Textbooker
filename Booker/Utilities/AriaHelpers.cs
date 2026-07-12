using System;

namespace Booker.Utilities;

/// <summary>
/// Helpers for rendering accessible markup consistently across Razor views.
/// </summary>
public static class AriaHelpers
{
    /// <summary>
    /// Returns the value to emit for an `aria-current` attribute, or null when
    /// the attribute should be omitted. Prevents emitting the literal string "null".
    /// </summary>
    /// <param name="activeFlag">Value produced by AdminNavPages/ManageNavPages (e.g. "page", null).</param>
    /// <returns>Non-empty flag, or null to suppress the attribute.</returns>
    public static string? CurrentOrNull(string? activeFlag)
    {
        return string.IsNullOrWhiteSpace(activeFlag) ? null : activeFlag;
    }

    /// <summary>
    /// Renders a boolean as the lowercase string required by the ARIA spec
    /// (e.g. for `aria-pressed`/`aria-expanded`), since Razor's `@value` would
    /// otherwise emit C#'s `True`/`False`.
    /// </summary>
    /// <param name="value">The boolean state to render.</param>
    /// <returns>"true" or "false".</returns>
    public static string Bool(bool value) => value ? "true" : "false";
}
