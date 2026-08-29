# 01. Nawigacja, stopka i jednolity adres kontaktowy

**Branch:** rodo-01-nawigacja-kontakt (patrz uwaga o nazewnictwie niżej)
**Zakres z audytu:** M2, przygotowanie pod H2 i H1
**Status:** zrobione

## Co zostało zmienione

- `Booker/Utilities/ContactInfo.cs` — nowy plik. Statyczna klasa `ContactInfo` z jedną stałą `SupportEmail = "support@textbooker.pl"`. Namespace `Booker.Utilities` jest już globalnie importowany w `Booker/Pages/_ViewImports.cshtml`, więc w widokach używa się jej bez prefiksu.
- `Booker/Pages/Shared/_Layout.cshtml` — trzy miejsca ze sztywno wpisanym `support@textbooker.pl` (link i tekst w panelu pomocy, link w stopce) zamienione na `@ContactInfo.SupportEmail`. W stopce dodano dwa nowe linki nawigacyjne: „Regulamin” (`asp-page="/Regulamin"`) i „Polityka prywatności w skrócie” (`asp-page="/Prywatnosc-w-skrocie"`), obok istniejącego linku do `/Privacy`.
- `Booker/Pages/Sitemap.cshtml` — w sekcji „Strony główne” dodano te same dwie pozycje: Regulamin i Polityka prywatności w skrócie, obok istniejącej Polityki prywatności.

## Decyzje, które podjąłem

- **Baza worktree: branch `rodo`, nie `main`.** Instrukcja zakładała `main` jako bazę. Sprawdziłem: `main` ma wyłącznie domyślną, szablonową stronę `Privacy.cshtml` ze scaffoldingu ASP.NET Core („Use this page to detail your site's privacy policy.”) — nie ma tam żadnej z treści, które opisuje audyt (art. 398, t.osmanowski@gmail.com, sekcje 1–11 itd.). Realna treść zgodna z audytem żyje na branchu `rodo` (bieżący branch repozytorium), który zawiera już 8 wcześniejszych commitów „Audyt RODO #1–#7” plus migrację. Uznałem to za oczywisty błąd założenia w instrukcji, a nie decyzję biznesową — użyłem `rodo` jako bazy dla worktree, żeby zadanie w ogóle miało sens (adres kontaktowy z audytu faktycznie tam występuje). Odnotowuję to jako największe ryzyko dla pozostałych 8 zadań — każde z nich też powinno wychodzić z `rodo`, nie z `main`.
- **Nazwa brancha: `rodo-01-nawigacja-kontakt` zamiast `rodo/01-nawigacja-kontakt`.** Git nie pozwala, by jednocześnie istniały referencje `refs/heads/rodo` i `refs/heads/rodo/...` (kolizja w hierarchii ref). Ponieważ branch `rodo` już istnieje w repozytorium, użyłem myślnika zamiast ukośnika jako separatora w nazwach wszystkich branchy zadań. Analogicznie należy nazwać branche 02–09: `rodo-02-regulamin`, `rodo-03-polityka` itd.
- **Miejsce prawdy dla adresu:** zwykła stała C# (`public const string`) w nowej klasie `Booker.Utilities.ContactInfo`, nie wpis w `appsettings.json`. Uzasadnienie: adres kontaktowy to wartość statyczna, niezmienna w runtime, nie różniąca się między środowiskami (Development/Production) — nie ma potrzeby przechodzić przez `IConfiguration` i wstrzykiwać go do widoków, które akurat tego nie robią (np. `Sitemap.cshtml` wstrzykuje `IConfiguration` tylko dla flag `Features:*`, ale to dodatkowa złożoność, której stała nie wymaga). Dzięki `@using Booker.Utilities` w `_ViewImports.cshtml` stała jest dostępna wprost we wszystkich stronach Razor bez dodatkowego kodu.
- **Linki do `/Regulamin` i `/Prywatnosc-w-skrocie` przez `asp-page`, nie przez sztywny `href`.** Obie strony jeszcze nie istnieją (powstaną w zadaniach 02 i 03). Przetestowałem zachowanie: dopóki strona docelowa nie istnieje, tag helper `asp-page` renderuje `href=""` (nie wyrzuca wyjątku). To gorsze niż zwykłe 404 z twardego linku, ale ma zaletę: po scaleniu branchy 02 i 03, kiedy strony faktycznie powstaną, linki zaczną działać automatycznie, bez dodatkowej edycji tego pliku. Uznałem to za właściwy wybór — jest to zgodne z resztą kodu (`Sitemap.cshtml` używa wyłącznie `asp-page`, nigdy sztywnych `href`).

## Na co natrafiłem

- Adres `no-reply@textbooker.pl` w `Booker/Services/SendMailSvc.cs:46` (nadawca systemowych e-maili, np. potwierdzeń) zostawiłem bez zmian — to nie jest adres kontaktowy administratora, tylko techniczny adres nadawcy, poza zakresem zadania (audyt mówił o „adresach e-mail administratora”).
- Prywatny e-mail `t.osmanowski@gmail.com` występuje w `Booker/Pages/Privacy.cshtml` w pięciu miejscach — zgodnie z poleceniem nie ruszałem tego pliku, zostaje dla zadania 03.
- Uruchomienie `dotnet run` lokalnie zadziałało bez problemu — projekt ma skonfigurowane połączenie z lokalną instancją SQL Server/LocalDB i bazę zainicjalizował sam (migracje + seed 50 ogłoszeń, 5 użytkowników), więc test end-to-end (curl po `https://localhost:5001/Sitemap` i stronie głównej) był możliwy bez dodatkowego przygotowania środowiska.

## Pliki poza przypisanym zakresem

Brak. Wszystkie zmiany mieszczą się w przypisanym zakresie zadania 01 (layout, stopka, panel pomocy, Sitemap) plus jeden nowy plik pomocniczy (`Booker/Utilities/ContactInfo.cs`), który nie koliduje z żadnym innym zadaniem z tabeli.

## Czego nie zrobiłem i dlaczego

- Nie tworzyłem stron `/Regulamin` ani `/Prywatnosc-w-skrocie` — zgodnie z wyraźnym poleceniem w zadaniu, powstaną w zadaniach 02 i 03; tu linki celowo prowadzą donikąd (`href=""`) do czasu scalenia.
- Nie ruszałem `Privacy.cshtml` — zarezerwowane dla zadania 03.

## Do decyzji właściciela

- **Baza worktree i nazewnictwo branchy.** Opisana wyżej rozbieżność (baza `main` vs `rodo`, oraz `rodo/NN-...` vs `rodo-NN-...`) dotyczy wszystkich 9 zadań, nie tylko tego. Zanim zaczną powstawać kolejne worktree, warto potwierdzić, że każde z nich ma wychodzić z brancha `rodo` (a nie `main`) i używać myślnika zamiast ukośnika w nazwie brancha — inaczej pozostałe zadania odziedziczą tę samą niespójność albo, gorzej, wyjdą z `main` i zgubią już wdrożone poprawki RODO #1–#7.

## Wyniki dodatkowego przeglądu (code review)

Przejrzałem cały diff (`git diff` + nowy plik `Booker/Utilities/ContactInfo.cs`) linijka po linijce, porównałem z konwencjami reszty repo i uruchomiłem pełny rebuild (`dotnet build --no-incremental`) w `Booker/` — **0 błędów, 0 ostrzeżeń**, także po pełnej rekompilacji widoków Razor (czyli `asp-page="/Regulamin"` i `asp-page="/Prywatnosc-w-skrocie"` nie psują builda mimo że strony docelowe jeszcze nie istnieją, zgodnie z tym co opisano wyżej).

Sprawdziłem dodatkowo (grep) czy gdziekolwiek indziej w kodzie zostały jeszcze twarde wystąpienia `support@textbooker.pl` poza nową stałą — nie znalazłem żadnych; wszystkie trzy miejsca w `_Layout.cshtml` zostały poprawnie zamienione na `@ContactInfo.SupportEmail`. Sprawdziłem też `Areas/Identity/Pages/Account/Register.cshtml` (osadzony tekst regulaminu) pod kątem adresu kontaktowego — nie zawiera żadnego, więc nie wchodzi w zakres tego zadania.

Nie znalazłem żadnych bugów, błędów logicznych ani problemów bezpieczeństwa w tym diffie — jest mały, spójny stylistycznie z resztą plików (te same wzorce `asp-area`/`asp-page` co w Sitemap i reszcie stopki) i nie wymagał żadnych poprawek kodu z mojej strony.

Jedna rzecz, którą zostawiam jako uwagę dla właściciela zamiast poprawiać samodzielnie (bo wymaga decyzji wykraczającej poza ten branch):

- **Nazwa trasy `/Prywatnosc-w-skrocie` odbiega od konwencji nazewnictwa Razor Pages reszty projektu.** Wszystkie istniejące nazwy stron w `Sitemap.cshtml`/`_Layout.cshtml` są w PascalCase bez myślników (`/Index`, `/Chat`, `/Add`, `/Profile/Index`, `/Privacy`, a także nowo dodane w tym zadaniu `/Regulamin`). Nazwa `/Prywatnosc-w-skrocie` z myślnikami i bez polskich znaków diakrytycznych jest jedynym wyjątkiem. Wewnętrznie jest to spójne — wszystkie 2 miejsca w tym diffie (stopka w `_Layout.cshtml`, `Sitemap.cshtml`) używają dokładnie tego samego stringa `/Prywatnosc-w-skrocie` — ale poprawność tego linku zależy od tego, jak plik `.cshtml` faktycznie nazwie zadanie 03 (np. `PrywatnoscWSkrocie.cshtml` dałoby inną trasę i link renderowałby się jako `href=""`, tak jak opisano w sekcji „Linki do `/Regulamin` i `/Prywatnosc-w-skrocie`” wyżej). Nie mogę tego zweryfikować ani poprawić z tego worktree bez zaglądania do brancha zadania 03, więc zostawiam to jako ryzyko koordynacyjne do potwierdzenia przy scalaniu branchy 01 i 03 — nazwa strony musi być identyczna w obu miejscach.
