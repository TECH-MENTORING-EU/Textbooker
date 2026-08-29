# Zgodność z RODO — działania podjęte w kodzie

Zwięzłe zestawienie, oparte na dowodach z kodu (branch `rodo` po scaleniu zadań 01–09). Nie jest
to rejestr czynności przetwarzania ani ocena skutków — to spis mechanizmów faktycznie
zaimplementowanych w aplikacji.

## Obowiązek informacyjny (art. 13)

- Pełna polityka prywatności: `Pages/Privacy.cshtml` — administrator z nazwy, podstawy prawne per
  kategoria danych, okresy przechowywania, prawa użytkownika.
- Wersja skrócona, jeden ekran: `Pages/Prywatnosc-w-skrocie.cshtml`, wzajemnie linkowana z pełną
  wersją.
- Regulamin jako osobna, trwale dostępna strona (nie tylko wklejony przy rejestracji tekst):
  `Pages/Regulamin.cshtml`, wersjonowany przez `Utilities/RegulaminInfo.CurrentVersion`.
- Klauzula informacyjna bezpośrednio nad formularzem rejestracji, wprost o widoczności e-maila:
  `Areas/Identity/Pages/Account/Register.cshtml` (sekcja `role="note"`).

## Zgoda i rozliczalność (art. 7 ust. 1, art. 8)

- Rejestracja wymaga jawnej akceptacji regulaminu i oświadczenia wiekowego — oba checkboxy
  walidowane po stronie serwera (`Input.AcceptTerms`, `Input.ConfirmsAgeRequirement`,
  `[Range(typeof(bool), "true", "true")]` w `Register.cshtml.cs`), nie tylko po stronie klienta.
- Moment i wersja zaakceptowanych dokumentów zapisywane przy koncie:
  `User.TermsAcceptedAt`, `User.TermsAcceptedVersion`, `User.AgeConfirmationAcceptedAt`
  (`Data/User.cs`), ustawiane w `Register.cshtml.cs::OnPostAsync`.
- Dane opcjonalne (telefon, WhatsApp, Messenger, Instagram, szkoła) startują wyłączone
  (`DisplayPhone`, `DisplayWhatsapp`, `DisplayMessenger`, `DisplayInstagram`, `DisplaySchool` —
  wszystkie `= false` domyślnie w `Data/User.cs`) i są przełączane niezależnie w ustawieniach
  profilu (`Areas/Identity/Pages/Account/Manage/Index.cshtml`) — wycofanie zgody to wyłączenie
  przełącznika.

## Małoletni użytkownicy (art. 8)

- Oświadczenie wieku (16 lat / zgoda opiekuna) wymagane przy rejestracji, opisane w
  `Pages/Privacy.cshtml`, sekcja „Małoletni użytkownicy”.

## Minimalizacja i kontrola widoczności danych

- Adres e-mail sprzedającego ujawniany wyłącznie zalogowanemu użytkownikowi i wyłącznie gdy
  sprzedający ma aktywne ogłoszenie: `ItemManager.HasVisibleListingAsync`, sprawdzane w
  `Pages/Profile/_Heading.cshtml` (`!Model.HasActiveListing` → komunikat zamiast danych).
- Nazwa szkoły ukryta domyślnie i widoczna tylko za zgodą właściciela profilu:
  `User.DisplaySchool`, respektowane w `Pages/Shared/_BookTile.cshtml` i `_Heading.cshtml`;
  szkoły nie da się zmienić z formularza ustawień — pole tylko do odczytu w
  `Manage/Index.cshtml.cs` (`SchoolName`, poza `[BindProperty] Input`).
- Ogłoszenia i profile z `noindex`, `wwwroot/robots.txt` blokuje `/Profile/` — potwierdzone
  wprost w treści `Pages/Privacy.cshtml`, pkt 5.

## Bezpieczeństwo przetwarzania (art. 32)

- Blokada logowania: 10 nieudanych prób → 2 godziny blokady, zerowane po sukcesie
  (`Program.cs`, `options.Lockout.MaxFailedAccessAttempts = 10`,
  `DefaultLockoutTimeSpan = TimeSpan.FromHours(2)`).
- Limit ujawniania danych kontaktowych: 60/godzinę, 200/dobę na konto, progi w konfiguracji
  (`appsettings.json` → `ContactRevealLimits`), egzekwowane w
  `Services/ContactRevealLimiter.cs`, wpięte w `Pages/Book.cshtml.cs`.
- Retry z logowaniem przy usuwaniu obiektów z Cloudflare R2 — błędy nie giną po cichu
  (`Services/PhotosManager.cs`, `Services/UserPhotoManager.cs`).

## Prawo dostępu i przenoszenia danych (art. 15 ust. 3, art. 20)

- Eksport danych osobowych w czytelnym, ustrukturyzowanym JSON (konto / ogłoszenia / ulubione),
  bez hasha hasła i tokenów bezpieczeństwa: `Areas/Identity/Pages/Account/Manage/DownloadPersonalData.cshtml.cs`.

## Prawo do usunięcia (art. 17)

- Usunięcie konta kasuje ogłoszenia, wyświetlenia i powiązania ulubionych (kaskady EF Core,
  `Data/DataContext.cs`) oraz obiekty w R2 (zdjęcie profilowe i zdjęcia ogłoszeń) —
  `Areas/Identity/Pages/Account/Manage/DeletePersonalData.cshtml.cs`,
  `Services/UserPhotoManager.cs`.

## Rozliczalność działań administratora (art. 5 ust. 2)

- Dziennik działań administracyjnych (blokada, odblokowanie, usunięcie konta, zmiana ról) w
  osobnej tabeli bez kluczy obcych z kaskadowym usuwaniem — wpis przetrwa usunięcie obiektu,
  którego dotyczył: `Data/AdminActionLog.cs`, podgląd w `Areas/Admin/Pages/AuditLog.cshtml`.

## Minimalizacja danych w treściach generowanych przez użytkowników

- Ostrzeżenia przy dodawaniu/edycji ogłoszenia, żeby nie umieszczać danych osobowych w zdjęciach
  i opisie: `Pages/Add.cshtml`, `Pages/Edit.cshtml`.
- Prosta heurystyka wykrywająca e-mail/telefon w opisie ogłoszenia — nie blokuje publikacji, tylko
  wymaga świadomego potwierdzenia i oznacza wpis do przeglądu: `Pages/Shared/ContentModerationHelper.cs`,
  `Data/Item.cs` (`FlaggedForReview`).

## Jeden punkt prawdy dla adresu kontaktowego

- `support@textbooker.pl` jako jedyny adres w całej aplikacji: `Utilities/ContactInfo.SupportEmail`,
  używany w stopce, panelu pomocy i polityce prywatności zamiast wklejonych na sztywno wartości.

---

Pełny kontekst decyzji i alternatyw dla każdego punktu: raporty `docs/rodo/01-*.md` … `09-*.md`.
Co pozostaje do zrobienia i czego ten dokument nie pokrywa: `docs/rodo/kolejne kroki.md`.
