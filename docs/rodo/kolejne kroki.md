# Kolejne kroki po scaleniu poprawek RODO

Stan na: scalenie wszystkich 9 branchy (`rodo-01-nawigacja-kontakt` … `rodo-09-dziennik-admina`)
do brancha `rodo`, wykonane 29.08.2026. Ten dokument opisuje, co zostało zrobione podczas
scalania, co wymaga jeszcze uwagi właściciela, i co zaginęło po drodze.

## 1. Co zostało zrobione przy scalaniu

Wszystkie 9 branchy zmergowane do `rodo`, w kolejności: 01 → 02 → 03 → 09 → 08 → 07 → 06 → 05 → 04
(kolejność dobrana tak, żeby konflikty pojawiały się względem już rozstrzygniętej, znanej
zawartości, a nie kumulowały się w ciemno). Rzeczywiste konflikty były dużo mniejsze niż sugerował
`git diff main..branch` dla każdego brancha z osobna — większość „różnic względem main” to po
prostu wspólna baza sprzed zadań 01–09, nie realne nakładanie się zmian. Realne konflikty:

- `.gitignore` (zadanie 01 vs baza) — scalono ręcznie, trywialne.
- `Areas/Admin/Pages/AdminNavPages.cs` i `_AdminNav.cshtml` (08 „Ogłoszenia” vs 09 „Dziennik
  działań”) — obie pozycje nawigacji zachowane.
- `Data/User.cs` (06 `DisplaySchool` vs 04 `TermsAcceptedAt`/`TermsAcceptedVersion`/
  `AgeConfirmationAcceptedAt`) — oba bloki zachowane, zgodnie z konwencją „blok na końcu klasy
  z komentarzem numeru zadania” z instrukcji.
- `Pages/Profile/Index.cshtml.cs` i `Favorites.cshtml.cs` (06 `SchoolName` vs 05
  `HasActiveListing` w tym samym rekordzie `UserModel`) — połączone w jeden rekord z obydwoma
  polami; `_Heading.cshtml` już poprawnie odwoływał się do obu (auto-merge).

Domknięty jednorazowy znacznik `// TODO scalanie:` w `Register.cshtml.cs` (zadanie 04) —
lokalna stała `AcceptedRegulaminVersion = "1.0"` zamieniona na `Booker.Utilities.RegulaminInfo.CurrentVersion`
(zadanie 02), zgodnie z tym, co znacznik zapowiadał.

**Migracje skonsolidowane.** Pięć migracji z poszczególnych zadań (`AddContactDisplayFlags`,
`AddTermsAndAgeAcceptance`, `AddDisplaySchool`, `AddItemFlaggedForReview`, `AddAdminActionLog`)
usunięte i zastąpione jedną: `20260829154428_RodoCompliancePack`. `DataContextModelSnapshot.cs`
wyszedł bit-identyczny z wersją sprzed konsolidacji — potwierdza, że to wyłącznie porządkowanie,
zero zmiany w wynikowym schemacie.

**Zweryfikowane uruchomieniowo, nie tylko kompilacyjnie.** `dotnet build` czysty (0/0). Aplikacja
uruchomiona lokalnie (`dotnet run`), migracja zaaplikowana bez błędu, dane deweloperskie
zaseedowane. Sprawdzone HTTP 200 na: `/`, `/Privacy`, `/Regulamin`, `/Prywatnosc-w-skrocie`,
`/Sitemap`, `/Identity/Account/Register`, `/Browse`, `/Book/1`, `/Profile/1`, `/robots.txt`;
`/Admin` poprawnie zwraca 404 dla niezalogowanego (oczekiwane).

## 2. Zaginiona wcześniejsza praca — do odtworzenia

Przed uruchomieniem serii zadań 01–09 ta sama sesja wykonała na branchu `rodo` dwa dodatkowe
kawałki pracy, które nigdy nie trafiły do żadnego commitu i zniknęły (najprawdopodobniej przy
resecie współdzielonego środowiska deweloperskiego, o którym wspomina `docs/rodo/00-podsumowanie.md`
w sekcji „Uwaga o środowisku lokalnym” — tam też zniknęła migracja `AddConsentTracking`, ślad tej
samej wcześniejszej pracy):

- **Nagłówki bezpieczeństwa i CSP w `Program.cs`** — `X-Content-Type-Options`, `Referrer-Policy`,
  `X-Frame-Options`, `Content-Security-Policy` (z `'unsafe-inline'` w script-src/style-src ze
  względu na inline `onclick`/`style` w widokach), plus reużycie istniejącej polityki
  `IpRateLimit` na `Login`/`ResetPassword`, plus usunięcie martwej polityki autoryzacji
  `AdminOnly`. Żadnego z tego nie ma dziś w kodzie.
- **Projekt testowy `Booker.Tests`** — dwa testy integracyjne (`WebApplicationFactory` + SQLite
  in-memory + Moq na `IAmazonS3`): usunięcie konta faktycznie kasuje powiązane dane i obiekty
  w R2, eksport danych zawiera wszystkie zainwentaryzowane pola. Katalog `Booker.Tests/` istnieje
  dziś na dysku, ale zawiera tylko `bin/`/`obj/` — same źródła zniknęły, nie ma ich w gicie.

Obie rzeczy są opisane szczegółowo w historii tej rozmowy i możliwe do odtworzenia w rozsądnym
czasie, jeśli właściciel zdecyduje się je odtworzyć.

## 3. Zidentyfikowany, nienaprawiony błąd (niezwiązany z żadnym z 9 zadań)

`Program.cs` ma `app.UseStatusCodePagesWithReExecute("/Status/{0}")`, a w `Pages/Status/` istnieje
tylko `404.cshtml`. Skutek: **każda odpowiedź z pustym body i kodem błędu innym niż 404 (400, 429,
cokolwiek) zostaje przekierowana na nieistniejącą stronę `/Status/{kod}`, co samo w sobie 404-uje —
maskując prawdziwy kod błędu jako nieodróżnialne, puste 404.** Odkryte niezależnie dwa razy:

- przez zadanie 08, przy próbie zweryfikowania POST-ów na `/Add`/`/Edit` przez `curl` — opisane
  w `docs/rodo/08-tresci-ogloszen.md`, sekcja „Na co natrafiłem”, jako coś reprodukującego się na
  kodzie sprzed zadania 08, nie jako wina tego zadania;
- wcześniej, w zaginionej pracy nad nagłówkami bezpieczeństwa (pkt 2 wyżej), przy okazji
  projektowania obsługi rate limitera — ten sam mechanizm miałby maskować odpowiedzi `429`.

**Rekomendacja:** dodać `Pages/Status/400.cshtml` i `Pages/Status/429.cshtml` (ten sam wzorzec co
istniejący `404.cshtml`), z krótkim, zrozumiałym komunikatem zamiast pustej strony. To osobne,
niezależne zadanie — nie wymaga nowego audytu, tylko dwóch nowych plików.

## 4. Otwarte decyzje właściciela (zebrane z raportów 01–09, wciąż aktualne)

- **Zmiana szkoły przez administratora** (zalecenie audytora z zadania 06) — świadomie pominięte
  we wszystkich 9 zadaniach. Wymaga osobnej decyzji, czy i kiedy to zaimplementować w panelu
  administracyjnym (najbliżej pasuje do `Areas/Admin/Pages/Users.cshtml` + dziennika z zadania 09).
- **Jeden wspólny komunikat błędu logowania** (zadanie 07) — zamiast osobnego, rozróżnialnego
  tekstu dla zablokowanego konta, zachowano jeden komunikat dla wszystkich przyczyn niepowodzenia,
  żeby nie osłabiać istniejącej ochrony przed enumeracją kont (`Login.cshtml.cs`, `GenericLoginFailureMessage`).
  To odejście od dosłownego brzmienia zadania 07 — do potwierdzenia.
- **Data wejścia w życie regulaminu** — ustawiona na 29 sierpnia 2026 r. (`RegulaminInfo.EffectiveDateDisplay`)
  jako techniczne założenie (data wykonania zadania 02), nie decyzja biznesowa. Do potwierdzenia
  lub zmiany na rzeczywistą datę wdrożenia.
- **Zdjęcia profilowe w polityce prywatności** — zadanie 03 wybrało wariant „funkcja nieaktywna,
  zdjęcia nie są prezentowane” (zamiast całkowitego usunięcia wzmianek o zdjęciu z treści). Do
  potwierdzenia, że to preferowane podejście, zwłaszcza jeśli funkcja profilowych zdjęć ma
  zostać w przyszłości włączona.
- **Ręczna weryfikacja formularzy `/Add` i `/Edit` w prawdziwej przeglądarce** — zalecana przez
  zadanie 08 z powodu nieprzetestowanego do końca zachowania POST-ów przez `curl` (patrz pkt 3).

## 5. Świadomie pominięte punkty audytu (bez zmian, zgodnie z instrukcją)

Z `docs/rodo/textbookerrodoinstrukcjeclaudecode.md`, sekcja „Świadomie pominięte”: M4 (szerszy
zakres eksportu danych, ID szkoły zamiast nazwy), M8 (nagłówki bezpieczeństwa — patrz jednak pkt 2
wyżej, ta praca istniała i zaginęła), M10 (okresy przechowywania dla kont zablokowanych i zgłoszeń
moderacyjnych), H6 (kasowanie zdjęć w R2 przy usuwaniu ogłoszenia/konta — nierozstrzygnięte, czeka
na wynik testu właściciela), oraz pozycje organizacyjne (rejestr czynności, umowy powierzenia,
procedura naruszeń, DPIA) — poza zakresem kodu.

## 6. Przypomnienie o bazie danych

Zgodnie z założeniem instrukcji, baza produkcyjna zostanie postawiona od zera przed wdrożeniem —
migracja `RodoCompliancePack` to jedyna migracja tej serii do zastosowania na świeżej bazie,
nie trzeba nic więcej godzić ani czyścić.
