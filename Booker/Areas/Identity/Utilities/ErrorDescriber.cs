using Microsoft.AspNetCore.Identity;

namespace Booker.Areas.Identity.Utilities
{
    public class ErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = "Wystąpił nieznany błąd." }; }
        public override IdentityError ConcurrencyFailure() { return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Błąd współbieżności, obiekt został zmodyfikowany." }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = "Nieprawidłowe hasło." }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = "Nieprawidłowy token." }; }
        public override IdentityError RecoveryCodeRedemptionFailed() { return new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Realizacja kodu odzyskiwania nie powiodła się." }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "To konto zewnętrzne jest już przypisane do innego użytkownika." }; }
        public override IdentityError InvalidUserName(string? userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = $"Nazwa użytkownika \"{userName}\" jest nieprawidłowa, może zawierać tylko litery i cyfry." }; }
        public override IdentityError InvalidEmail(string? email) { return new IdentityError { Code = nameof(InvalidEmail), Description = $"E-mail \"{email}\" jest nieprawidłowy." }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = $"Nazwa użytkownika \"{userName}\" jest już zajęta." }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = $"E-mail \"{email}\" jest już zajęty." }; }
        public override IdentityError InvalidRoleName(string? role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = $"Nazwa roli \"'{role}'\" jest nieprawidłowa." }; }
        public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Nazwa roli \"{role}\" jest już zajęta." }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "Użytkownik ma już ustawione hasło." }; }
        public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Blokada nie jest włączona dla tego użytkownika." }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Użytkownik ma już przypisaną rolę \"{role}\"." }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = $"Użytkownik nie ma przypisanej roli \"{role}\"." }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = $"Hasło musi mieć co najmniej {length} {(DoubleNumber(length) ? "znaki" : "znaków")}." }; }
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) { return new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Hasło musi zawierać co najmniej {uniqueChars} {(DoubleNumber(uniqueChars) ? "różne znaki" : "różnych znaków")}." }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Hasło musi zawierać co najmniej jeden znak specjalny." }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Hasło musi zawierać co najmniej jedną cyfrę (0-9)." }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Hasło musi zawierać co najmniej jedną małą literę (a-z)." }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Hasło musi zawierać co najmniej jedną wielką literę (A-Z)." }; }

        // In Polish, different suffix is used between 2-4 and 5+ numbers (e.g. "2 znaki", "5 znaków").
        // Of course, this description is an oversimplification.
        private bool DoubleNumber(int number)
        {
            if (number < 2)
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be greater than 1.");

            number %= 100;
            if (number >= 10 && number <= 20)
            {
                return false; // 11, 12, 13, ..., 19
            }
            else
            {
                number %= 10;
                return number >= 2 && number <= 4; // 2, 3, 4
            }
        }
    }
}
