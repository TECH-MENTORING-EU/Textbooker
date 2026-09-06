using System.ComponentModel.DataAnnotations;

namespace Booker.Utilities;

// Server-only validator for bool fields that must be explicitly checked (e.g. consent
// checkboxes). [Required] is a no-op on non-nullable bool, and [Range(typeof(bool),"true","true")]
// used to stand in for it - but the client-side validation library parses range bounds with
// parseFloat, so parseFloat("True") is NaN and the check always failed, blocking every
// submission. This attribute intentionally has no client-side counterpart; the checkbox's
// HTML5 "required" attribute covers client-side feedback instead.
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is bool b && b;
    }
}
