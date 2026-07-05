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
}
