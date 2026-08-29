# 06. Szkoła: przełącznik widoczności, własna szkoła w profilu, brak zmiany

**Branch:** rodo-06-szkola (baza: `rodo`, nie `main` — patrz raport zadania 01)
**Zakres z audytu:** H4
**Status:** zrobione

## Co zostało zmienione

- `Booker/Data/User.cs` — dodano blok `// RODO — zadanie 06`: `DisplaySchool` (`bool`, domyślnie `false`), `[PersonalData]`.
- `Booker/Migrations/*_AddDisplaySchool.cs` (+ `.Designer.cs`, `DataContextModelSnapshot.cs`) — migracja dodająca kolumnę.
- `Booker/Pages/Book.cshtml` — karta sprzedającego pokazuje nazwę szkoły tylko, gdy `User.DisplaySchool == true` (wcześniej pokazywała zawsze zalogowanym, z fallbackiem „Bez szkoły”).
- `Booker/Pages/Shared/_BookTile.cshtml` — analogiczna poprawka na kartach ogłoszeń (galeria, wyniki wyszukiwania, lista w profilu).
- `Booker/Areas/Identity/Pages/Account/Manage/Index.cshtml` i `.cshtml.cs` — dodano: pole „Szkoła” tylko do odczytu (z adnotacją o braku możliwości zmiany) oraz przełącznik „Pokaż moją szkołę przy moich ogłoszeniach” (domyślnie wyłączony). Nazwa szkoły jest osobną, niewiązaną (`[BindProperty]`-free) właściwością `SchoolName` — nie da się jej nadpisać przez POST.
- `Booker/Pages/Profile/Index.cshtml.cs` i `Booker/Pages/Profile/Favorites.cshtml.cs` — `UserModel` rozszerzony o `SchoolName` (dociągane osobno przez `DataContext`, bo `UserManager` nie ładuje nawigacji `User.School`).
- `Booker/Pages/Profile/_Heading.cshtml` — na własnym profilu zawsze widoczna własna szkoła z adnotacją „nie można jej zmienić”; na cudzym profilu szkoła widoczna tylko, gdy właściciel profilu ma włączony `DisplaySchool`.

## Decyzje, które podjąłem

- **Domyślna wartość `DisplaySchool = false`** — zgodnie z decyzją właściciela.
- **Właściciel profilu zawsze widzi własną szkołę, niezależnie od przełącznika** — również zgodnie z decyzją; zaimplementowane jako osobna gałąź `if (Model.IsCurrentUser)` w `_Heading.cshtml`, więc przełącznik `DisplaySchool` steruje wyłącznie tym, co widzą **inni**.
- **Nazwa szkoły jako osobna, niewiązana właściwość (`SchoolName`), nie pole w `InputModel`.** To bezpośrednio realizuje wymóg punktu 5 zadania („sprawdź, czy formularz ustawień nie przyjmuje pola szkoły z żądania”) — `SchoolId`/`School` nigdy nie istnieją w klasie `[BindProperty] Input`, więc model binder nie ma czego przypisać, nawet gdyby atakujący dołożył pole `Input.SchoolId` do żądania POST. Przetestowałem to bezpośrednio (patrz „Weryfikacja”).
- **Dociąganie nazwy szkoły przez `DataContext.Schools.FindAsync(SchoolId)`, nie przez nawigację `user.School`.** Sprawdziłem: `UserManager<User>.FindByIdAsync`/`GetUserAsync` (domyślny `UserStore` z Identity) nie robi `.Include(u => u.School)` — nawigacja zostałaby `null` nawet dla użytkownika z przypisaną szkołą. Zamiast zmieniać zachowanie `UserManager` (ryzykowne, dotyka wielu miejsc), dociągnąłem nazwę szkoły osobnym, tanim zapytaniem po `SchoolId` w każdym miejscu, gdzie jest potrzebna.
- **Ukrycie pozycji „Bez szkoły” zamiast pokazywania jej przy wyłączonym przełączniku.** Wcześniej `Book.cshtml`/`_BookTile.cshtml` pokazywały „Bez szkoły” dla użytkowników bez przypisanej szkoły. Przy nowym modelu (przełącznik widoczności) uznałem, że pokazywanie czegokolwiek o szkole przy wyłączonym `DisplaySchool` byłoby niespójne — teraz sekcja szkoły po prostu nie renderuje się wcale, gdy `DisplaySchool == false` **lub** użytkownik nie ma przypisanej szkoły.
- **Zalecenie audytora (zmiana szkoły przez admina) — pominięte, patrz „Do decyzji właściciela”.**

## Na co natrafiłem

- Potwierdziłem bezpośrednim testem, że `UserManager` faktycznie nie ładuje `User.School` — próba wyświetlenia `Model.RequestUser.School?.Name` bez osobnego zapytania zwróciłaby zawsze `null`, nawet dla użytkowników z przypisaną szkołą. To nie jest błąd w istniejącym kodzie (nikt wcześniej nie próbował renderować szkoły przez `UserManager`-owe obiekty `User` — `Book.cshtml`/`_BookTile.cshtml` korzystają z `Item.User.School`, ładowanego przez `ItemManager` z jawnym `.Include(...).ThenInclude(u => u.School)`), ale warto o tym pamiętać przy przyszłych zmianach korzystających z `UserManager`.
- Środowisko deweloperskie: ponownie natrafiłem na pułapkę `dotnet run --no-build` uruchomiony zaraz po `dotnet ef migrations add` — nowo wygenerowany plik migracji nie trafia automatycznie do już skompilowanego DLL-a, więc `Database.Migrate()` nie widział nowej migracji jako oczekującej, mimo że model C# (i generowane zapytania SQL) już odwoływał się do nowej kolumny. Rozwiązanie: zwykły `dotnet build` przed uruchomieniem. To błąd kolejności poleceń z mojej strony, nie problem w kodzie repozytorium.

## Pliki poza przypisanym zakresem

Brak. Wszystkie zmiany mieszczą się w przypisanym zakresie zadania 06 („ustawienia profilu (szkoła), widoki ogłoszeń i profilu, model użytkownika”).

## Czego nie zrobiłem i dlaczego

- **Zalecenie audytora — zmiana szkoły przez administratora na zgłoszenie e-mail — nie zaimplementowałem.** Wymagałoby to edycji `Areas/Admin/Pages/Users.cshtml` i `Users.cshtml.cs`, które w tabeli zakresów należą wprost do **zadania 09** („strony panelu administracyjnego”). Zadanie 09 najpewniej i tak edytuje ten sam plik (ukrycie przycisku „Więcej…”, dziennik działań administracyjnych obejmujący m.in. „operacje na szkołach, jeśli panel je udostępnia” — to dosłownie ta sama funkcja, którą miałbym tu dodać). Zaimplementowanie jej tutaj niezależnie od 09 gwarantowałoby konflikt przy scalaniu na dokładnie tym samym pliku, prawdopodobnie w tym samym miejscu (lista akcji na wierszu użytkownika). Uznałem, że to wyraźnie wykracza poza zakres tego zadania — zgodnie z instrukcją zadania, pomijam i zapisuję jako pytanie do właściciela poniżej.

## Do decyzji właściciela

- **Zmiana szkoły przez administratora (zalecenie audytora z tego zadania).** Rekomenduję zrealizować to w ramach **zadania 09**, a nie tutaj — z dwóch powodów: (1) dotyka pliku `Areas/Admin/Pages/Users.cshtml(.cs)`, który zadanie 09 i tak edytuje; (2) zadanie 09 wprowadza dziennik działań administracyjnych, który zgodnie ze swoim opisem ma obejmować „operacje na szkołach, jeśli panel je udostępnia” — czyli zakłada, że taka operacja może istnieć, i naturalnie powinna zostać zalogowana tym samym mechanizmem. Zaimplementowanie jej osobno w zadaniu 06 rozdzieliłoby funkcję od jej własnego dziennika audytowego.

## Wyniki dodatkowego przeglądu (code review)

Przegląd objął cały `git diff`/`git status` w tym worktree (9 zmienionych plików + 2 pliki migracji) — nic więcej. Ogólna ocena: implementacja poprawna, bez luki typu overposting.

### Zmiany zastosowane bezpośrednio

- **`Booker/Areas/Identity/Pages/Account/Manage/Index.cshtml`** — pole „Szkoła” (tylko do odczytu) miało `<label>` bez atrybutu `for` i `<input>` bez `id`, więc etykieta nie była programowo powiązana z polem (drobny problem dostępności; pole `Username` obok, jako jedyny inny przykład pola `disabled` w tym pliku, ma to powiązane automatycznie przez `asp-for`). Dodałem `id="school-name"` do `<input>` i `for="school-name"` do `<label>`.

### Weryfikacja braku overpostingu `SchoolId`

Przejrzałem cały diff pod tym kątem punkt po punkcie: `InputModel` w `Manage/Index.cshtml.cs` nie zawiera `SchoolId` ani `School` — jedyne miejsce, gdzie nazwa szkoły trafia do widoku, to osobna, niewiązana właściwość `SchoolName` na `PageModel`, ustawiana wyłącznie po stronie serwera z `user.SchoolId` (odczyt, nie zapis). `OnPostAsync` przepisuje do `user` tylko pola z `Input` (`PhoneNumber`, `DisplayEmail`, …, `DisplaySchool`) — `user.SchoolId` nigdzie nie jest przypisywane z danych żądania. Sprawdziłem też `Areas/Admin/Pages/Users.cshtml.cs` — nie zawiera pola `SchoolId` w żadnym bindowalnym modelu, więc również tam nie ma ścieżki do zmiany szkoły (zgodnie z opisem w raporcie, że administracyjna zmiana szkoły nie istnieje jeszcze w kodzie). Nie znalazłem żadnej ścieżki overpostingu wprowadzonej przez ten diff.

### Rozbieżność ze specyfikacją zadania — brak `HasVisibleListingAsync`

Zlecenie przeglądu wymieniało jako jeden z elementów do sprawdzenia „nowy helper `HasVisibleListingAsync` na `ItemManager`”. W tym diffie (i w ogóle w repozytorium na tym branchu) taka metoda **nie istnieje** — `Booker/Services/ItemManager.cs` w ogóle nie jest częścią zmian tego zadania. Zamiast scentralizowanego helpera, warunek widoczności szkoły (`DisplaySchool && School != null`, dodatkowo pod `User.Identity?.IsAuthenticated == true`) jest zduplikowany inline w dwóch miejscach: `Book.cshtml` i `_BookTile.cshtml`. Efekt końcowy jest poprawny i spójny (sprawdziłem oba miejsca renderujące `Item.User.School` — nie ma trzeciego), ale:
1. albo opis zadania odnosił się do podejścia, którego autor tego worktree świadomie nie zastosował (wybierając prostszy inline-check zamiast helpera na `ItemManager`),
2. albo to rzeczywisty brak w realizacji.

Nie wprowadziłem tu żadnej zmiany — dodanie nowej metody na `ItemManager` tylko po to, by zduplikowany dwuwarunkowy `if` zamienić na jedno wywołanie, to decyzja architektoniczna (i ewentualnie dodatkowe zapytanie/async w miejscu, gdzie dziś wystarczy właściwość w pamięci), a nie oczywista poprawka — zostawiam to właścicielowi zadania do oceny, czy brak helpera jest akceptowalny, czy wymaga uzupełnienia.

### Inne obserwacje (bez zmian)

- Dociąganie `SchoolName` przez `_context.Schools.FindAsync(...)` w trzech miejscach (`Manage/Index.cshtml.cs`, `Profile/Index.cshtml.cs`, `Profile/Favorites.cshtml.cs`) jest lekko zduplikowane (ten sam trzy-liniowy wzorzec `user.SchoolId.HasValue ? ... : null` powtórzony trzykrotnie). Można by to wydzielić do wspólnej metody (np. na `DataContext` albo osobnym serwisie), ale to pojedyncze, tanie zapytanie po kluczu głównym w każdym miejscu — nie uznałem tego za błąd ani za wystarczająco pilne, by zmieniać bez decyzji właściciela; zostawiam jako obserwację.
- Sprawdziłem oba call-site'y rekordu `UserModel` (dodano trzeci parametr `SchoolName`) — `Profile/Index.cshtml.cs` i `Profile/Favorites.cshtml.cs` — oba zaktualizowane spójnie, kompilacja przechodzi.
- Kolejność właściwości w `DataContextModelSnapshot.cs` i pliku `.Designer.cs` zachowuje porządek alfabetyczny zgodny z konwencją istniejącą w pliku (`DisplayPhone` → `DisplaySchool` → `DisplayWhatsapp`).

### Build

`dotnet build` w `Booker/` po zmianach: **Build succeeded, 0 Warning(s), 0 Error(s)**.
