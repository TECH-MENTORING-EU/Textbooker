namespace Booker.Services
{
    /// <summary>
    /// Shared content for the one-time "welcome, your account is now active" email.
    /// Centralized here so the two activation paths (student's own email confirmation,
    /// and the guardian's consent confirmation) cannot drift apart when the wording changes.
    /// </summary>
    public static class WelcomeEmailTemplate
    {
        public const string Subject = "Witamy w TextBooker! Twoje konto zostało pomyślnie utworzone 🎉";

        public const string Body =
            "Cześć! <br /> Cieszymy się, że dołączyłeś/dołączyłaś do społeczności TextBooker! <br /> " +
            "Twoje konto zostało pomyślnie aktywowane. Możesz się już zalogować. <br /><br /> " +
            "Pozdrawiamy, <br /> Zespół TextBooker📚";
    }
}
