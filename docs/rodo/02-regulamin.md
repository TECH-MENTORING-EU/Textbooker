# 02. Regulamin jako osobna strona serwisu

**Branch:** rodo-02-regulamin (baza: `rodo`, nie `main` — patrz raport zadania 01)
**Zakres z audytu:** H2 (w całości)
**Status:** zrobione

## Co zostało zmienione

- `Booker/Pages/Regulamin.cshtml` — nowa strona publiczna `/Regulamin`, w tym samym stylu co `Privacy.cshtml` (te same klasy: `<article class="privacy-policy">`, nagłówki `h2`/`h3`... właściwie tu zostawiłem oryginalny układ `<p><strong>&sect; N.</strong></p>` + `<ol>`, bo tak wyglądał regulamin w widoku rejestracji — patrz sekcja „Decyzje”). Zawiera pełną treść §1–§9 przeniesioną z `Register.cshtml`, plus trzy nowe postanowienia (§1b, §7a) i przebudowany §8.
- `Booker/Pages/Regulamin.cshtml.cs` — pusty `PageModel` (`RegulaminModel`), analogiczny do `PrivacyModel`.
- `Booker/Utilities/RegulaminInfo.cs` — nowa statyczna klasa ze stałymi wersji regulaminu.

## Decyzje, które podjąłem

- **Zachowanie oryginalnego układu treści zamiast konwersji na `h2`.** W widoku rejestracji regulamin był sformatowany jako `<p><strong>&sect; N. Tytuł</strong></p>` + `<ol>`, a nie jako nagłówki `h2`. Uznałem to za formatowanie, a nie postanowienie merytoryczne, więc mogłem je poprawić — ale zdecydowałem się zachować oryginalny wzorzec (paragraf ze znakiem §, potem lista numerowana), bo tak wygląda regulamin w innych serwisach i tekst prawny czyta się w tym układzie naturalnie; `Privacy.cshtml` używa `h2`, ale to inny dokument z innym stylem numeracji (1., 2., 3. zamiast §). Nie uznałem rozbieżności stylu nagłówków między dwoma dokumentami prawnymi za problem wymagający ujednolicenia w tym zadaniu.
- **Miejsce nowych postanowień:** dodałem je jako `§ 1b. Szkoła Użytkownika` (zaraz po `§ 1a. Wiek Użytkowników`, bo to analogiczne postanowienie „kto i jak może korzystać z konta”) oraz `§ 7a. Zdjęcia profilowe` (zaraz po `§ 7. Czego nie wolno robić?`, przed sekcją o danych osobowych). Zachowuje to czytelność numeracji (analogiczny wzorzec „1a” już istniał w oryginalnym tekście) i nie wymusza przenumerowania całego dokumentu.
- **§8 zastąpiony odesłaniem.** Zgodnie z poleceniem, cały dotychczasowy §8 „Dane osobowe i polityka prywatności” (4 zdawkowe punkty) zastąpiłem jednym akapitem z linkiem `asp-page="/Privacy"` i zastrzeżeniem, że w razie sprzeczności rozstrzyga polityka prywatności.
- **Stała wersji: `Booker.Utilities.RegulaminInfo.CurrentVersion` (string, wartość `"1.0"`), plik `Booker/Utilities/RegulaminInfo.cs`.** Dodatkowo `RegulaminInfo.EffectiveDateDisplay` (string, `"29 sierpnia 2026 r."`) do wyświetlenia daty wejścia w życie na stronie. Nazwa i lokalizacja są jawne pod tym samym `namespace Booker.Utilities`, który jest już globalnie zaimportowany w `_ViewImports.cshtml` — zadanie 04 może odwołać się do `RegulaminInfo.CurrentVersion` bez dodatkowego `using`, o ile scalanie zachowa ten plik. Datę wejścia w życie ustawiłem na dzisiejszą datę (29.08.2026) jako datę powstania tej strony — to założenie, nie fakt biznesowy; właściciel może ją zmienić przy wdrożeniu.
- **Numer wersji `1.0`:** to pierwsza opublikowana, samodzielna wersja regulaminu (wcześniej regulamin nie istniał jako osobny dokument z wersją, tylko jako tekst wklejony w rejestracji) — naturalny punkt startowy.
- **`noindex`:** sprawdziłem, że `Privacy.cshtml` **nie ma** ustawionego `ViewData["Robots"] = "noindex"` (w przeciwieństwie do `Book.cshtml`, `Profile/Index.cshtml`, `Profile/Favorites.cshtml`, które mają). Zgodnie z poleceniem zadania („ustaw noindex, jeśli tak samo skonfigurowana jest polityka prywatności; w przeciwnym razie zostaw indeksowanie domyślne”) **nie dodałem** `noindex` do Regulaminu.

## Na co natrafiłem

- Treść regulaminu w `Register.cshtml` była sformatowana z użyciem encji `&sect;` i cudzysłowów prostych (`"tablicy ogłoszeniowej"`) zamiast typograficznych. Przy kopiowaniu ujednoliciłem cudzysłowy na `„…”`, zgodnie z resztą tekstów prawnych w serwisie (Privacy.cshtml konsekwentnie używa `„…”`) — to poprawka formatowania, nie treści, zgodnie z dozwolonym zakresem zmian.
- Zadanie 01 (wykonane wcześniej w tej samej sesji, inny worktree) już przygotowało linki do `/Regulamin` w stopce i mapie strony przez `asp-page`, więc po scaleniu obu branchy linki zaczną działać automatycznie bez dodatkowej edycji — potwierdziłem to rozumowanie uruchamiając aplikację i sprawdzając, że `/Regulamin` faktycznie zwraca 200 i renderuje się w domyślnym layoucie.

## Pliki poza przypisanym zakresem

Brak. Zmiany ograniczają się do nowej strony `Regulamin.cshtml` + `Regulamin.cshtml.cs` i nowej stałej `RegulaminInfo.cs` — wszystko mieści się w przypisanym zakresie zadania 02 („nowa strona Regulamin + jej treść i stała wersji”).

## Czego nie zrobiłem i dlaczego

- Nie ruszałem `Register.cshtml` — zgodnie z wyraźnym poleceniem, usunięcie wklejonej treści regulaminu i podlinkowanie `/Regulamin` należy do zadania 04. Regulamin będzie chwilowo istniał w dwóch miejscach do czasu scalenia.
- Nie dodawałem stylów CSS specyficznych dla `/Regulamin` — strona dziedziczy generyczne style Pico.css po klasie `privacy-policy`, tak jak `Privacy.cshtml` (który też nie ma dedykowanego arkusza stylów).

## Do decyzji właściciela

- Data wejścia w życie regulaminu (`RegulaminInfo.EffectiveDateDisplay`) ustawiona na 29 sierpnia 2026 r. to techniczne założenie (data wykonania tego zadania), nie decyzja biznesowa — do potwierdzenia lub zmiany przez właściciela przed wdrożeniem produkcyjnym.

## Wyniki dodatkowego przeglądu (code review)

Sprawdziłem cały diff (trzy nowe pliki: `Regulamin.cshtml`, `Regulamin.cshtml.cs`, `RegulaminInfo.cs`) pod kątem błędów, spójności z resztą serwisu i wierności skopiowanej treści prawnej.

**Zweryfikowane i poprawne (bez zmian):**
- Treść §1–§9 skopiowana z `Areas/Identity/Pages/Account/Register.cshtml` jest wierna oryginałowi znak w znak (poza celowo ujednoliconymi cudzysłowami `„…"` — potwierdziłem bajtowo, że użyto właściwych encji U+201E/U+201D, zgodnie z `Privacy.cshtml`).
- Wszystkie tagi HTML (`article`, `ol`, `ul`, `li`, `p`, `strong`) domykają się parami — policzyłem wystąpienia skryptem, brak niedomkniętych elementów.
- Konwencja `ViewData["Title"] = "X - TextBooker"` zgodna z resztą stron (`Privacy.cshtml`, `Error.cshtml`, `Sitemap.cshtml`).
- Brak `ViewData["Robots"] = "noindex"` jest prawidłowy — potwierdziłem, że `Privacy.cshtml` też go nie ma (w przeciwieństwie do `Book.cshtml`, `Profile/Index.cshtml`, `Profile/Favorites.cshtml`), więc zachowanie domyślnego indeksowania jest spójne z poleceniem zadania.
- `RegulaminInfo` w `namespace Booker.Utilities` jest poprawnie dostępny w Razorze bez dodatkowego `@using`, bo `Booker.Utilities` jest już zaimportowany globalnie w `_ViewImports.cshtml`.
- **§7a (zdjęcia profilowe) jest faktograficznie zgodny z kodem**: sprawdziłem `appsettings.json` i `appsettings.Development.json` — `Features:ProfilePhotosEnabled` = `false` w obu, a `Book.cshtml` rzeczywiście warunkuje wyświetlanie zdjęcia tą flagą. Treść „funkcja jest nieaktywna, zdjęcia nie są prezentowane innym Użytkownikom” jest prawdziwa.

**Znaleziony problem — nie naprawiłem, wymaga decyzji:**
- **§1b („Szkoła Użytkownika”) opisuje mechanizm, który nie istnieje w kodzie.** Tekst mówi: „Nazwa szkoły Użytkownika może być prezentowana przy jego ogłoszeniach innym Użytkownikom Serwisu wyłącznie wtedy, gdy Użytkownik samodzielnie włączy tę opcję w ustawieniach profilu. Domyślnie widoczność szkoły jest wyłączona.” Sprawdziłem `Data/User.cs` — istnieją pola `DisplayEmail`, `DisplayPhone`, `DisplayWhatsapp`, `DisplayMessenger`, `DisplayInstagram`, ale **nie ma** żadnego `DisplaySchool` ani analogicznego przełącznika. Rzeczywiste zachowanie (sprawdzone w `Pages/Book.cshtml:106-109`, `Pages/Shared/_BookTile.cshtml:35-42`) jest inne: nazwa szkoły jest ukrywana tylko przed niezalogowanymi (anonimowymi) odwiedzającymi (`User.Identity?.IsAuthenticated == true`), a pokazywana **każdemu zalogowanemu użytkownikowi serwisu** — nie ma żadnego ustawienia „w profilu”, którym właściciel konta mógłby to włączyć/wyłączyć per-użytkownik.
  - Nie poprawiłem tego samodzielnie, bo to nie jest literówka ani drobna niespójność formatowania — to rozbieżność między treścią dokumentu prawnego a faktycznym działaniem aplikacji, a poprawny zapis zależy od decyzji biznesowej: czy opisywać stan faktyczny („szkoła widoczna dla zalogowanych, ukryta dla anonimowych, bez możliwości zmiany”), czy potraktować to jako zapowiedź funkcji, która ma dopiero powstać (przełącznik w profilu, analogiczny do `DisplayMessenger`/`DisplayInstagram`) i zbudować brakującą funkcjonalność w osobnym zadaniu.
  - Rekomendacja: przed wdrożeniem produkcyjnym albo (a) przeredagować §1b tak, by opisywał rzeczywisty mechanizm oparty o `IsAuthenticated`, albo (b) dodać pole `DisplaySchool` do `User` (analogicznie do istniejących `Display*`) i UI w ustawieniach profilu, żeby treść regulaminu była prawdziwa od dnia publikacji. Publikowanie regulaminu obiecującego funkcję, która nie istnieje, jest ryzykiem prawnym (wprowadzenie w błąd konsumenta), więc uznałem to za coś do rozstrzygnięcia przez właściciela/scalenie zadań, a nie do cichej naprawy w tym worktree.

**Build:** `dotnet build` w `Booker/` — 0 błędów, 0 ostrzeżeń, bez zmian w kodzie z mojej strony (przegląd nie wymagał żadnych poprawek poza udokumentowaniem powyższego).
