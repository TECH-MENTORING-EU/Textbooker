# 04. Rejestracja: klauzula informacyjna, linki i utrwalenie oświadczeń

**Branch:** rodo-04-rejestracja (baza: `rodo`, nie `main` — patrz raport zadania 01)
**Zakres z audytu:** H1 (w całości), H2 (część), M5 (w całości)
**Status:** zrobione

## Co zostało zmienione

- `Booker/Areas/Identity/Pages/Account/Register.cshtml`:
  - Usunięto wklejoną treść regulaminu (§1–§9). W miejscu checkboxa akceptacji dodano link do `/Regulamin` (`target="_blank"`).
  - Dodano blok klauzuli informacyjnej nad przyciskiem rejestracji: administrator, cel przetwarzania, jawne zdanie o widoczności e-maila po kliknięciu przycisku kontaktu, zdanie o braku możliwości zmiany szkoły, oraz dwa linki — „Polityka prywatności w skrócie” (wyeksponowana, pogrubiona, pierwsza) i „Pełna polityka prywatności”.
  - Dodano atrybut HTML `required` do obu checkboxów (`acceptTermsCheckbox`, `confirmsAgeRequirementCheckbox`) — wcześniej nie miał go żaden z nich.
- `Booker/Areas/Identity/Pages/Account/Register.cshtml.cs`:
  - `Input.AcceptTerms` — dodano `[Range(typeof(bool), "true", "true", ...)]` (naprawa luki opisanej niżej).
  - Dodano stałą `AcceptedRegulaminVersion = "1.0"` w klasie `RegisterModel`, z komentarzem `// TODO scalanie: użyć stałej z zadania 02`.
  - W `OnPostAsync`, tuż przed zapisem użytkownika, dodano ustawienie `user.TermsAcceptedAt`, `user.TermsAcceptedVersion`, `user.AgeConfirmationAcceptedAt` na wspólny znacznik czasu `now`.
- `Booker/Data/User.cs` — dodano blok `// RODO — zadanie 04` z trzema nowymi właściwościami: `TermsAcceptedAt` (`DateTime?`), `TermsAcceptedVersion` (`string?`), `AgeConfirmationAcceptedAt` (`DateTime?`), wszystkie oznaczone `[PersonalData]`.
- `Booker/Migrations/20260829110518_AddTermsAndAgeAcceptance.cs` (+ `.Designer.cs`, `DataContextModelSnapshot.cs`) — nowa migracja EF dodająca te trzy kolumny do `AspNetUsers`.

## Decyzje, które podjąłem

- **Przyczyna luki w walidacji (do raportu — to była właściwa diagnoza, nie zgadywanie).** `Input.AcceptTerms` miał tylko `[Required]`, a `[Required]` na nienullowalnym `bool` w ASP.NET Core **nigdy nie zawodzi** — `Required` sprawdza tylko `null`, a `default(bool)` to `false`, więc pole zawsze „ma wartość”. `Input.ConfirmsAgeRequirement` działał poprawnie wyłącznie dzięki dodatkowemu atrybutowi `[Range(typeof(bool), "true", "true")]`, którego `AcceptTerms` nie miał. Naprawa: dodanie identycznego `[Range]` do `AcceptTerms`. To dokładnie odtwarza „wzorzec do naśladowania” wskazany w treści zadania.
- **Stała wersji regulaminu — deklaracja lokalna z `// TODO scalanie`.** Zgodnie z poleceniem, ponieważ branch zadania 02 (`Booker.Utilities.RegulaminInfo.CurrentVersion = "1.0"`) nie jest widoczny w tym worktree, zadeklarowałem własną stałą `AcceptedRegulaminVersion = "1.0"` bezpośrednio w klasie `RegisterModel` (a nie jako osobny plik w `Booker/Utilities`, żeby nie wychodzić poza przypisany zakres „strony Register.*”). Wartość `"1.0"` dobrałem zgodnie z tym, co zadanie 02 faktycznie ustawiło (widziałem to wcześniej w tej samej sesji) — po scaleniu obu branchy wystarczy podmienić tę stałą na `RegulaminInfo.CurrentVersion`.
- **Treść klauzuli informacyjnej — zwięzła, bez wymieniania wszystkich podstaw prawnych.** Zawarłem dokładnie to, czego wymagało zadanie: administrator, cel, komu dane będą pokazane (z naciskiem na e-mail + przycisk kontaktu), prawa (dostęp/sprostowanie/usunięcie), niemożność zmiany szkoły, oraz dwa linki w określonej kolejności i hierarchii wizualnej (skrót pogrubiony i pierwszy). Nie kopiowałem pełnej klauzuli z polityki prywatności — uznałem to za właściwe, bo zadanie wprost prosi o „zwięzłą, kilka zdań” wersję, a szczegóły i tak są dostępne pod linkami.
- **Wspólny znacznik czasu `now` dla obu oświadczeń.** Zamiast wywoływać `DateTime.Now` trzykrotnie (dla `TermsAcceptedAt` i `AgeConfirmationAcceptedAt`), użyłem jednej zmiennej — oba oświadczenia są składane w tym samym momencie (jeden formularz, jedno kliknięcie „Zarejestruj”), więc rozbieżne znaczniki czasu nie miałyby żadnej wartości informacyjnej, a jedna zmienna jest prostsza.
- **Migracja: `AddTermsAndAgeAcceptance`, wygenerowana standardowo przez `dotnet ef migrations add`**, bez żadnych ręcznych modyfikacji SQL — zgodnie z zasadą „nie próbuj godzić migracji między zadaniami”, migracja jest czystym odzwierciedleniem zmian w `User.cs`.

## Na co natrafiłem

- **Lokalna baza deweloperska (Docker, `sql2022-dev`, `localhost:1433`) zawierała nierozpoznaną migrację `20260828215041_AddConsentTracking`**, której nie ma w żadnym pliku na branchu `rodo` ani w żadnym z moich worktree — dodawała m.in. kolumnę `TermsAcceptedAt` do `AspNetUsers`. To spowodowało kolizję nazw kolumn przy próbie zastosowania mojej migracji (`AddTermsAndAgeAcceptance` też dodaje `TermsAcceptedAt`). Wygląda na pozostałość po jakiejś wcześniejszej, niezwiązanej z tym zadaniem próbie w tym samym środowisku deweloperskim. **Za zgodą użytkownika zresetowałem lokalną bazę `Booker`** (zawiera wyłącznie dane testowe/seedowane, odtwarzane przy każdym starcie aplikacji — potwierdzone w instrukcji zadania: „baza produkcyjna będzie postawiona od zera”). Po resecie wszystkie migracje, łącznie z moją, zastosowały się czysto od zera. To nie dotyczy żadnej bazy współdzielonej ani produkcyjnej — wyłącznie mojego lokalnego środowiska testowego w tej sesji.
- **Realny connection string dla lokalnej bazy jest w `dotnet user-secrets`, nie w `appsettings.Development.json`** — ten drugi ma tylko nieużywany fallback z `Trusted_Connection=true` (który w tym środowisku w ogóle nie działa, bo brak Kerberosa). To ważne dla kogokolwiek, kto będzie chciał ręcznie łączyć się z lokalną bazą deweloperską poza samą aplikacją.
- Sam formularz rejestracji nie ma żadnego globalnego atrybutu `[Authorize]` do sprawdzenia — jest domyślnie publiczny, więc nie trzeba było nic zmieniać w tym zakresie.

## Weryfikacja

Uruchomiłem aplikację lokalnie i wykonałem dokładnie test opisany w zadaniu:

1. **POST bez `Input.AcceptTerms`**, z wolną nazwą użytkownika i wolnym e-mailem, ale z `Input.ConfirmsAgeRequirement=true`: serwer zwrócił `200 OK` (ponowne wyrenderowanie formularza, nie przekierowanie), ze span walidacyjnym `data-valmsg-for="Input.AcceptTerms"` zawierającym tekst „Musisz zaakceptować regulamin.”. Zapytanie do bazy potwierdziło: **0 kont** utworzonych dla tej nazwy użytkownika. **Konto nie powstaje** — luka jest zamknięta.
2. **POST z oboma polami zaakceptowanymi** (`AcceptTerms=true`, `ConfirmsAgeRequirement=true`): serwer zwrócił `302` do `/Identity/Account/RegisterConfirmation`. Zapytanie do bazy potwierdziło zapisany rekord: `TermsAcceptedAt` i `AgeConfirmationAcceptedAt` ustawione na moment rejestracji, `TermsAcceptedVersion = "1.0"`.

## Pliki poza przypisanym zakresem

Brak. Zmiany ograniczają się do `Register.cshtml`, `Register.cshtml.cs`, `Data/User.cs` i nowej migracji — dokładnie zakres przypisany zadaniu 04.

## Czego nie zrobiłem i dlaczego

- Nie tworzyłem osobnego pliku stałej w `Booker/Utilities` dla wersji regulaminu — zgodnie z poleceniem, zadeklarowałem ją lokalnie w `RegisterModel` z jawnym `// TODO scalanie`, żeby nie wychodzić poza przypisany zakres plików.
- Nie zmieniałem treści e-maila potwierdzającego rejestrację (`_emailSender.SendEmailAsync(...)` w `Register.cshtml.cs`) — nie było to częścią zakresu zadania, a treść nie zawiera nieprawdziwych informacji o danych osobowych.

## Do decyzji właściciela

- Migracja `20260828215041_AddConsentTracking` widoczna w historii lokalnej bazy deweloperskiej (przed resetem) sugeruje, że ktoś już wcześniej próbował wdrożyć podobne śledzenie zgód w tym środowisku, poza tym zadaniem. Warto sprawdzić, czy to pozostałość po nieudokumentowanym eksperymencie, zanim rozpocznie się scalanie branchy — może zawierać podejście, z którym warto się zapoznać, albo być całkowicie nieaktualna i do zignorowania.

## Wyniki dodatkowego przeglądu (code review)

Przejrzałem cały `git diff` tego worktree (te same pliki, które wymienia sekcja „Co zostało zmienione” powyżej). Logika walidacji (`[Range(typeof(bool), "true", "true")]` na `Input.AcceptTerms`), model `User` i migracja EF są poprawne, spójne z istniejącą konwencją (`ConfirmsAgeRequirement`, `CreatedAt` używa `DateTime.Now`, więc `DateTime.Now` dla nowych znaczników czasu też jest spójny) i nie znalazłem w nich błędów wymagających poprawki. `dotnet build` w `Booker/`: **0 błędów, 0 ostrzeżeń** — bez żadnych zmian z mojej strony, bo nie było tu nic prostego do naprawienia; poniższe dwie rzeczy są zbyt duże/niepewne, żeby poprawiać je bez decyzji właściciela.

**1. Dwa nowe linki w `Register.cshtml` wskazują na strony, których nie ma w tym repozytorium (nawet po uwzględnieniu bazy `rodo`).**

`asp-page="/Regulamin"` (linia 86) i `asp-page="/Prywatnosc-w-skrocie"` (linia 77) nie odpowiadają żadnej istniejącej stronie Razor Pages — sprawdziłem całe drzewo `Booker/Pages` i `Booker/Areas`: istnieje wyłącznie `Pages/Privacy.cshtml` (użyte poprawnie jako `/Privacy`). Potwierdziłem to też przez `grep` — te dwa ciągi (`Regulamin`, `Prywatnosc-w-skrocie`) nie występują nigdzie indziej w kodzie poza tym jednym miejscem.

To nie jest błąd kompilacji — `asp-page` jest rozwiązywane dopiero w runtime przez `LinkGenerator`. Subagent code-review, który dopisał poniższy akapit, założył (bez uruchomienia aplikacji), że `AnchorTagHelper` z `asp-page` wskazującym na nieistniejącą stronę rzuca `InvalidOperationException` i psuje całą stronę `/Identity/Account/Register`. **Sprawdziłem to empirycznie i to nieprawda:** uruchomiłem aplikację i pobrałem `GET /Identity/Account/Register` — strona zwraca `200 OK` i renderuje się w całości; oba linki po prostu dostają puste `href=""` (`<a ... href="">regulamin</a>` i `<a ... href="">Polityka prywatności w skrócie</a>`), bez żadnego wyjątku. To dokładnie takie samo zachowanie, jakie zweryfikowano już w raporcie zadania 01 dla tych samych dwóch tras w stopce/mapie strony. Strona rejestracji **nie jest zepsuta** w tym worktree — linki po prostu nie prowadzą jeszcze donikąd, co jest oczekiwane do czasu scalenia z zadaniami 02 i 03, dokładnie tak jak already udokumentowano w raporcie zadania 01. Nie ma tu żadnego realnego ryzyka wymagającego dodatkowej rekomendacji poza tym, co zadanie 01 już zapisało.

**2. Treść klauzuli informacyjnej może nieprecyzyjnie opisywać, kiedy e-mail staje się widoczny.**

Klauzula (linia 67-69) mówi: „Twój adres e-mail zobaczą zalogowani użytkownicy, którzy klikną przycisk kontaktu przy Twoim ogłoszeniu”. Sprawdziłem faktyczny mechanizm: `Pages/Profile/_Heading.cshtml` renderuje sekcję „Kontakt” (a w niej `Pages/Shared/_ContactDetails.cshtml`, która pokazuje e-mail, gdy `DisplayEmail == true` — czyli domyślnie dla każdego nowego konta) dla **każdego zalogowanego użytkownika odwiedzającego profil**, bez żadnego „przycisku kontaktu” pośredniczącego w ogłoszeniu — nie znalazłem w kodzie żadnego elementu UI, który pasowałby do opisu „kliknij przycisk kontaktu przy ogłoszeniu”. Innymi słowy, dostęp do e-maila może być łatwiejszy niż sugeruje klauzula (wystarczy być zalogowanym i wejść na profil sprzedającego, niekoniecznie poprzez konkretne ogłoszenie i przycisk). To dotyczy treści zadania 04 (H1 — klauzula informacyjna ma być zgodna z rzeczywistością przetwarzania), ale nie plików, które zostały zmienione w tym zadaniu (mechanizm kontaktu żyje w `Pages/Profile` i `Pages/Shared`, poza przypisanym zakresem). Nie zmieniałem treści klauzuli samodzielnie, bo to decyzja co do faktycznego opisu funkcjonalności produktu, a nie oczywista poprawka kodu — **rekomendacja: właściciel powinien albo doprecyzować tekst klauzuli (usunąć/skorygować wzmiankę o „przycisku kontaktu”, jeśli mechanizm faktycznie działa przez odwiedzenie profilu), albo potwierdzić, że w międzyczasie (inne zadanie z tej serii) mechanizm kontaktu faktycznie zostanie ukryty za przyciskiem/interakcją, co uzasadniałoby obecne sformułowanie.**
