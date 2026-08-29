namespace Booker.Services;

// RODO - task 07: generous but finite limits on revealing a seller's contact details.
// Counted per account, in process memory (see ContactRevealLimiter) - every reveal counts,
// including viewing the contact for the same listing again.
public class ContactRevealLimitOptions
{
    public int PerHour { get; set; } = 60;
    public int PerDay { get; set; } = 200;
}
