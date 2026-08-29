# TextBooker — wdrożenie poprawek RODO. Instrukcja dla Claude Code

Dokument wejściowy: `textbooker-audyt-rodo.md` (audyt z 29.08.2026). Poniższe zadania realizują **decyzje właściciela serwisu** podjęte na podstawie tego audytu. Decyzje są wiążące — nie podważaj ich, nawet jeśli widzisz lepsze rozwiązanie; zamiast tego zanotuj uwagę w raporcie zadania.

---

## Zasady obowiązujące we wszystkich zadaniach

### Git

- **Jedno zadanie = jeden branch = jeden worktree.** Każdy worktree zakładasz z tej samej bazy (`main`; jeśli domyślny branch nazywa się inaczej, użyj jego).
- Zadania wykonujesz **po kolei**, w podanej numeracji. Nie zaczynaj kolejnego, zanim nie zamkniesz poprzedniego raportem.
- **Nie commituj niczego.** Zmiany zostają w katalogu roboczym worktree. Nie rób `git add`, `git commit`, `git push`, `git merge`, `git rebase`. Nie zmieniaj plików w głównym worktree repozytorium.

```bash
# na początku każdego zadania, z katalogu głównego repo:
git worktree add -b rodo/NN-krotka-nazwa ../tb-wt/NN-krotka-nazwa main
cd ../tb-wt/NN-krotka-nazwa
```

Nazwy branchy i katalogów są podane przy każdym zadaniu.

### Samodzielność

Podejmuj decyzje implementacyjne sam — nazwy klas, układ plików, sposób walidacji, treść komunikatów UI. Nie dopytuj. Jeśli natrafisz na wybór, którego nie da się rozstrzygnąć bez wiedzy biznesowej, wybierz wariant bezpieczniejszy dla użytkownika, zaimplementuj go i **opisz rozterkę w raporcie**.

### Podział na pliki — ważne

Ponieważ nic nie jest commitowane, kolejne zadania **nie widzą zmian z poprzednich**. Zadania zostały rozdzielone tak, żeby nie edytowały tych samych plików. Trzymaj się przypisanego zakresu:

| Zadanie | Pliki, które **wolno** Ci zmieniać |
|---|---|
| 01 | layout, stopka, panel pomocy, `Sitemap`, konfiguracja adresu kontaktowego |
| 02 | nowa strona `Regulamin` + jej treść i stała wersji |
| 03 | `Privacy` + nowa strona ze skróconą polityką |
| 04 | strony `Register.*`, model użytkownika, migracja |
| 05 | logika ujawniania kontaktu, ustawienia profilu (`Manage`), model użytkownika |
| 06 | ustawienia profilu (szkoła), widoki ogłoszeń i profilu, model użytkownika |
| 07 | konfiguracja Identity / rate limiting, `Program.cs`, `appsettings` |
| 08 | strony dodawania i edycji ogłoszenia |
| 09 | strony panelu administracyjnego, nowa encja dziennika |

Jeżeli musisz wyjść poza swój zakres — **zrób to**, ale wypisz w raporcie każdy plik spoza tabeli, żeby dało się przewidzieć konflikt scalania.

Zadania 04, 05, 06 i 09 dodają właściwości do modelu użytkownika lub nowe encje. Dodawaj je jako **zwarty blok na końcu klasy, poprzedzony komentarzem z numerem zadania** — wtedy konflikt przy scalaniu jest trywialny.

### Migracje EF

Baza produkcyjna **będzie postawiona od zera przed wdrożeniem** — nie ma danych do zachowania i nie ma potrzeby dbać o historię migracji. Każde zadanie może dodać własną migrację; po scaleniu wszystkich branchy właściciel usunie nagromadzone migracje i wygeneruje jedną. Nie próbuj godzić migracji między zadaniami.

### Weryfikacja przed zamknięciem zadania

1. `dotnet build` przechodzi bez błędów i bez nowych ostrzeżeń.
2. Aplikacja startuje, a ścieżki dotknięte zmianą działają (uruchom i sprawdź ręcznie albo `curl`-em).
3. Nie dodałeś nowych zależności NuGet/npm bez odnotowania tego w raporcie.
4. Teksty w UI są po polsku, spójne stylistycznie z resztą serwisu.

### Raport z zadania — obowiązkowy

Na koniec każdego zadania utwórz w swoim worktree plik:

```
docs/rodo/NN-krotka-nazwa.md
```

Struktura:

```markdown
# NN. <tytuł zadania>

**Branch:** rodo/NN-krotka-nazwa
**Zakres z audytu:** <symbole, np. H1, M5>
**Status:** zrobione / zrobione częściowo / zablokowane

## Co zostało zmienione
Lista plików ze zwięzłym opisem zmiany w każdym.

## Decyzje, które podjąłem
Każda decyzja: co wybrałem, jakie były alternatywy, dlaczego ta.
Uwzględnij nazwy, teksty komunikatów, wartości domyślne, progi liczbowe.

## Na co natrafiłem
Niespodzianki w kodzie, długi techniczne, rzeczy niezgodne z opisem w audycie,
miejsca, gdzie kod robił coś innego, niż zakładało zadanie.

## Pliki poza przypisanym zakresem
Lista albo „brak”. To jest ostrzeżenie o ryzyku konfliktu przy scalaniu.

## Czego nie zrobiłem i dlaczego
Świadome pominięcia, rzeczy odłożone, blokery.

## Do decyzji właściciela
Pytania, których nie dało się rozstrzygnąć samodzielnie. Może być puste.
```

Raport pisz konkretnie — to jedyny ślad po zadaniu, bo commitów nie ma.

---

## Kontekst techniczny

ASP.NET Core, Razor Pages, ASP.NET Core Identity, htmx, EF Core. Zdjęcia w Cloudflare R2. UI po polsku. Serwis obsługuje ogłoszenia sprzedaży używanych podręczników szkolnych; użytkownikami są uczniowie, w tym niepełnoletni.

Adres kontaktowy obowiązujący w całym serwisie: **support@textbooker.pl**.

---

# Zadanie 01 — Nawigacja, stopka i jednolity adres kontaktowy

**Branch/worktree:** `rodo/01-nawigacja-kontakt`
**Realizuje:** M2, przygotowanie pod H2 i H1

## Cel

W serwisie krążą dwa różne adresy kontaktowe: polityka prywatności podaje prywatny gmail, a stopka i panel pomocy `support@textbooker.pl`. Ma zostać jeden.

## Zakres

1. Przeszukaj repozytorium pod kątem **wszystkich** wystąpień adresów e-mail administratora (w widokach, konfiguracji, treściach, szablonach wiadomości, `appsettings`). Zamień na `support@textbooker.pl`.
2. Wprowadź **jedno miejsce prawdy** dla tego adresu — stała w konfiguracji albo w klasie ustawień — i używaj jej w widokach zamiast wpisywać adres na sztywno. Polityka prywatności ma swój własny tekst i zajmie się nią zadanie 03; tutaj nie ruszaj `Privacy`.
3. W stopce dodaj link **„Regulamin"** prowadzący do `/Regulamin` oraz zostaw istniejący link do polityki prywatności. Obok dodaj link do skróconej wersji polityki pod adresem `/Prywatnosc-w-skrocie`.
4. W mapie strony (`/Sitemap`) dodaj te same trzy pozycje w sekcji stron głównych.

## Uwaga

Strony `/Regulamin` i `/Prywatnosc-w-skrocie` powstaną w zadaniach 02 i 03. Do czasu scalenia branchy linki będą prowadzić do 404 — to oczekiwane, nie próbuj tego obchodzić ani tworzyć tych stron tutaj.

## Ukończone, gdy

Żaden inny adres kontaktowy nie występuje już w kodzie poza `Privacy` (którą pomijasz), a stopka i mapa strony mają komplet linków.

---

# Zadanie 02 — Regulamin jako osobna strona serwisu

**Branch/worktree:** `rodo/02-regulamin`
**Realizuje:** H2 (w całości)

## Problem

Regulamin istnieje wyłącznie jako tekst wklejony w stronę rejestracji. Po założeniu konta użytkownik nie ma jak wrócić do treści, którą zaakceptował. Ustawa o świadczeniu usług drogą elektroniczną wymaga udostępnienia regulaminu w sposób umożliwiający jego pozyskanie, odtworzenie i utrwalenie.

## Zakres

1. Utwórz stronę `/Regulamin`, publicznie dostępną **bez logowania**, w tym samym layoucie co polityka prywatności.
2. Przenieś do niej pełną treść regulaminu (§1–§9) z widoku rejestracji. Treść skopiuj wiernie — poprawiaj wyłącznie literówki i formatowanie, nie zmieniaj postanowień. **Wyjątek:** §8 „Dane osobowe i polityka prywatności" zastąp krótkim odesłaniem do polityki prywatności z linkiem do `/Privacy` — pełną klauzulę informacyjną niesie polityka, a duplikowanie jej w regulaminie prowadzi do rozjazdu wersji.
3. Dopisz do regulaminu następujące postanowienia (decyzje właściciela):
   - **Zmiana szkoły:** szkołę wybiera się jednorazowo przy zakładaniu konta i nie można jej później zmienić; zmiana szkoły wymaga założenia nowego konta.
   - **Widoczność szkoły:** nazwa szkoły użytkownika może być widoczna przy jego ogłoszeniach, jeżeli użytkownik włączy tę opcję w ustawieniach profilu; domyślnie jest wyłączona. Zalogowany użytkownik zawsze widzi własną szkołę we własnym profilu.
   - **Zdjęcia profilowe:** funkcja zdjęć profilowych jest w tej wersji serwisu nieaktywna i zdjęcia profilowe nie są prezentowane innym użytkownikom.
4. Dodaj na górze strony **datę wejścia w życie** i oznaczenie wersji (np. `1.0`). Wersję trzymaj jako stałą w kodzie w jednym miejscu — zadanie 04 będzie ją zapisywać przy rejestracji, więc udostępnij ją jako publiczną stałą o czytelnej nazwie i **opisz w raporcie jej dokładną nazwę i lokalizację**.
5. Ustaw stronie `noindex`, jeśli tak samo skonfigurowana jest polityka prywatności; w przeciwnym razie zostaw indeksowanie domyślne.

## Czego nie robisz

Nie dotykasz stron rejestracji — usunięcie wklejonej treści i podlinkowanie regulaminu należy do zadania 04. Na tym branchu regulamin będzie chwilowo w dwóch miejscach.

## Ukończone, gdy

`/Regulamin` otwiera się bez logowania, zawiera pełną treść z trzema nowymi postanowieniami, ma wersję i datę, a stała wersji jest gotowa do użycia przez zadanie 04.

---

# Zadanie 03 — Polityka prywatności: korekty i wersja skrócona

**Branch/worktree:** `rodo/03-polityka`
**Realizuje:** M1, M2 (w treści polityki), M3, część H1, zapisy dokumentacyjne dla H3, H4, M6, M9

## Część A — poprawki w istniejącej polityce (`/Privacy`)

1. **Podstawa prawna cookies (M1).** W pkt 11 jest „art. 398 ustawy — Prawo komunikacji elektronicznej". Poprawna podstawa dla plików niezbędnych to **art. 399 ust. 3** tej ustawy. Popraw numer, resztę wywodu zostaw.
2. **Adres kontaktowy (M2).** Zamień wszystkie wystąpienia `t.osmanowski@gmail.com` na `support@textbooker.pl`. Dane administratora (imię, nazwisko, nazwa działalności) zostają bez zmian.
3. **Urealnienie pkt 5 (M3).** Obecny tekst twierdzi, że ogłoszenia i profile są dostępne „dla każdego odwiedzającego Serwis, także bez logowania" i „mogą być indeksowane przez wyszukiwarki". Serwis działa inaczej i chroni użytkowników lepiej: strony ogłoszeń i profili mają `noindex`, `robots.txt` blokuje `/Profile/`, a dane kontaktowe sprzedającego są dostępne **wyłącznie po zalogowaniu**, po kliknięciu przycisku kontaktu. Przepisz pkt 5 tak, żeby opisywał stan faktyczny. Zachowaj natomiast — bo to prawda — ostrzeżenie, że treści raz pokazane innym użytkownikom mogą zostać przez nich skopiowane i rozpowszechnione poza kontrolą administratora, oraz akapit o tym, że po przeniesieniu kontaktu poza serwis każda ze stron staje się samodzielnym administratorem.
4. **Podstawa prawna widoczności e-maila (H3, ścieżka A).** W pkt 4 przenieś udostępnianie **adresu e-mail sprzedającego** z art. 6 ust. 1 lit. a (zgoda) na **art. 6 ust. 1 lit. b** — wykonanie umowy. Uzasadnienie do wpisania: publikacja ogłoszenia służy nawiązaniu kontaktu z zainteresowanym, a udostępnienie kanału kontaktu jest niezbędne do świadczenia usługi. Zaznacz, że dotyczy to użytkowników, którzy mają opublikowane ogłoszenie, i że dane widzi wyłącznie zalogowany użytkownik po kliknięciu przycisku kontaktu.
   Na zgodzie (art. 6 ust. 1 lit. a) **zostają** dane opcjonalne: numer telefonu, WhatsApp, Messenger, Instagram. Opisz je osobno, wraz z informacją o możliwości wycofania zgody przez wyłączenie widoczności.
5. **Szkoła (H4).** W pkt 3 i 5 dopisz, że nazwa szkoły może być prezentowana przy ogłoszeniach użytkownika, jeżeli włączy on tę opcję (domyślnie wyłączona), oraz że szkoły nie można zmienić po założeniu konta — zmiana wymaga nowego konta.
6. **Zdjęcia profilowe (M9).** Usuń z pkt 3 i pkt 4 zdjęcie profilowe i wizerunek jako dane przetwarzane i objęte zgodą, albo dopisz wyraźnie, że funkcja jest nieaktywna i zdjęcia nie są prezentowane. Wybierz wariant i uzasadnij w raporcie.
7. **Zakres danych ogłoszenia (M6).** Pkt 3 opisuje dane ogłoszenia jako „zdjęcia podręczników, opis, cena i stan przedmiotu". Formularz przyjmuje dowolne zdjęcia i dowolny tekst. Przeformułuj tak, żeby obejmowało treści wprowadzone przez użytkownika, wraz z zastrzeżeniem, że nie należy umieszczać w nich danych osobowych ani wizerunku osób.
8. **Odbiorcy danych (H7).** W pkt 6 dodaj: dostawcę usługi formularzy (Google — formularz opinii, dane mogą być przetwarzane na serwerach poza EOG) oraz akapit o profilach serwisu na Facebooku i Instagramie wraz z informacją o współadministrowaniu danymi statystycznymi odwiedzających z Meta.
9. Zaktualizuj datę ostatniej aktualizacji na dole dokumentu.

## Część B — skrócona wersja polityki (H1)

Utwórz stronę `/Prywatnosc-w-skrocie`, publicznie dostępną bez logowania.

- **Nazwa:** „Polityka prywatności w skrócie" albo „Najważniejsze w skrócie". **Nie nazywaj tego wersją dla dzieci, dla młodszych ani dla uczniów** — z tej strony mają korzystać także rodzice i każdy, kto chce zrozumieć rzecz szybko. Nic w tytule ani w treści nie może sugerować, że to wersja gorsza albo dla kogoś mniej rozgarniętego.
- **Objętość:** jeden ekran, maksymalnie ok. 400 słów.
- **Język:** krótkie zdania, druga osoba, bez numerów artykułów w treści głównej.
- **Treść — odpowiedz na pytania w tej kolejności:** kto prowadzi serwis i jak się z nim skontaktować; jakie dane zbieramy; co widzą inni użytkownicy (osobno wyróżnij e-mail i szkołę); czego inni nie widzą; komu przekazujemy dane; jak długo je trzymamy; co możesz zrobić ze swoimi danymi (pobrać, poprawić, usunąć konto — z linkami do odpowiednich stron ustawień); gdzie się poskarżyć.
- Na dole wyraźny link: „Pełna treść polityki prywatności" → `/Privacy`. Na `/Privacy` dodaj u góry odwrotny link do wersji skróconej.
- Ta strona **nie zastępuje** polityki pełnej i nie może z nią być sprzeczna. Po napisaniu przejdź obie i sprawdź zgodność.

## Ukończone, gdy

Polityka nie zawiera już nieprawdziwych ani nieaktualnych twierdzeń z listy powyżej, a skrócona wersja daje się przeczytać w minutę i zgadza się z pełną.

---

# Zadanie 04 — Rejestracja: klauzula informacyjna, linki i utrwalenie oświadczeń

**Branch/worktree:** `rodo/04-rejestracja`
**Realizuje:** H1 (w całości), H2 (część), M5 (w całości)

## Problem

Formularz rejestracji nie zawiera żadnej informacji o przetwarzaniu danych ani linku do polityki prywatności. Do tego **akceptacja regulaminu nie jest walidowana po stronie serwera** — potwierdzone testem: żądanie `POST` bez pola `Input.AcceptTerms` przechodzi walidację modelu i zatrzymuje się dopiero na sprawdzeniu unikalności loginu. Oświadczenie wiekowe jest walidowane prawidłowo i stanowi wzorzec do naśladowania.

## Zakres

1. **Usuń wklejoną treść regulaminu** z widoku rejestracji. W jej miejsce daj link do `/Regulamin` otwierany w nowej karcie, przy checkboxie akceptacji.
2. **Dodaj klauzulę informacyjną** nad przyciskiem rejestracji — zwięzłą, kilka zdań, obejmującą: kto jest administratorem, w jakim celu przetwarza dane, komu dane będą pokazane, oraz że użytkownik ma prawo dostępu, sprostowania i usunięcia danych. Zakończ dwoma linkami: **„Polityka prywatności w skrócie"** (`/Prywatnosc-w-skrocie`) jako pierwszym i bardziej wyeksponowanym oraz **„Pełna polityka prywatności"** (`/Privacy`).
   W klauzuli musi paść wprost — prostym językiem — że **adres e-mail użytkownika zobaczą zalogowani użytkownicy, którzy klikną przycisk kontaktu przy jego ogłoszeniu**. To jest informacja, której dziś brakuje najbardziej, i nie może zginąć w drobnym druku.
   Napisz też jednym zdaniem, że **wybranej szkoły nie da się później zmienić** — użytkownik ma to wiedzieć w momencie wyboru, a nie po fakcie.
3. **Walidacja serwerowa akceptacji regulaminu.** Wymuś `Input.AcceptTerms` po stronie serwera dokładnie tak, jak zrobione jest to dla `Input.ConfirmsAgeRequirement`. Komunikat błędu po polsku. Sprawdź, czy oba checkboxy są też oznaczone jako wymagane w HTML, żeby walidacja kliencka zadziałała wcześniej.
4. **Utrwalenie oświadczeń.** Dodaj do modelu użytkownika (blok na końcu klasy, z komentarzem `// RODO — zadanie 04`):
   - datę i godzinę akceptacji regulaminu,
   - **wersję regulaminu**, którą zaakceptowano — pobierz ją ze stałej utworzonej w zadaniu 02. Ponieważ tamten branch nie jest tu widoczny, zadeklaruj własną stałą o tej samej roli, oznacz ją w kodzie komentarzem `// TODO scalanie: użyć stałej z zadania 02` i **opisz to w raporcie**.
   - datę i godzinę złożenia oświadczenia wiekowego.
   Zapisuj te wartości w momencie tworzenia konta. Dodaj migrację.
5. **Weryfikacja.** Powtórz test, który wykrył lukę: wyślij `POST` na rejestrację z wolną nazwą i wolnym e-mailem, ale bez pola `Input.AcceptTerms`. Konto **nie może powstać**. Wynik testu opisz w raporcie.

## Ukończone, gdy

Rejestracji nie da się przejść bez zaznaczenia obu oświadczeń, oba są zapisane w bazie z datą i wersją dokumentu, a użytkownik przed kliknięciem „Zarejestruj" wie, że jego e-mail będzie widoczny i że szkoły nie zmieni.

---

# Zadanie 05 — Widoczność danych kontaktowych: podstawa prawna i domyślne ustawienia

**Branch/worktree:** `rodo/05-kontakt-podstawa`
**Realizuje:** H3, ścieżka A

## Decyzja właściciela

Udostępnienie **adresu e-mail** sprzedającego przestaje być oparte na zgodzie i opiera się na wykonaniu umowy (art. 6 ust. 1 lit. b RODO). Dzięki temu domyślne włączenie tej widoczności przestaje być wadą — nie ma zgody, która mogłaby być nieważna. **Dane opcjonalne pozostają na zgodzie i muszą być domyślnie wyłączone.**

Baza zostanie postawiona od nowa przed wdrożeniem — nie migrujesz istniejących kont i nie piszesz skryptów naprawczych.

## Zakres

1. **Wartości domyślne w modelu użytkownika.** Ustaw jawnie, w jednym czytelnym miejscu:
   - widoczność e-maila: **`true`**,
   - widoczność telefonu, WhatsAppa, Messengera, Instagrama: **`false`**,
   - publiczne ulubione: **`false`** (już tak jest — potwierdź).
   Obecny kod ma widoczność telefonu domyślnie `true` mimo braku numeru w profilu; to trzeba poprawić.
2. **Zawężenie do sprzedających.** Podstawa „wykonanie umowy" broni się wobec osoby, która wystawiła ogłoszenie. Zadbaj, żeby dane kontaktowe były ujawniane **wyłącznie w kontekście ogłoszenia** i wyłącznie zalogowanemu użytkownikowi. Sprawdź, czy profil użytkownika bez żadnego aktywnego ogłoszenia nie pokazuje jego adresu e-mail — jeśli pokazuje, ogranicz to.
3. **Spójność przełączników z rzeczywistością.** Endpoint ujawniający kontakt ma respektować każdą z flag widoczności z osobna. Przetestuj to i opisz wynik w raporcie — w audycie tego nie udało się rozstrzygnąć.
4. **Przebudowa ustawień profilu.** Przełącznik e-maila przestaje być zgodą, a staje się **wyborem kanału kontaktu**. Przeredaguj etykiety i opisy pomocnicze w `Ustawieniach profilu`:
   - przy e-mailu: informacja, co się stanie po włączeniu i po wyłączeniu, oraz ostrzeżenie, że wyłączenie wszystkich kanałów sprawi, że nikt nie skontaktuje się w sprawie ogłoszeń;
   - przy danych opcjonalnych: jasne oznaczenie, że są dobrowolne, a włączenie widoczności jest zgodą, którą można w każdej chwili wycofać.
5. **Ostrzeżenie przy publikacji.** Jeśli użytkownik ma wyłączone **wszystkie** kanały kontaktu, a dodaje ogłoszenie — pokaż widoczne ostrzeżenie z linkiem do ustawień. Ogłoszenie bez żadnego kanału kontaktu jest bezużyteczne dla obu stron.

## Ukończone, gdy

Domyślne wartości są jawne i zgodne z decyzją, dane opcjonalne startują wyłączone, każdy przełącznik faktycznie steruje tym, co obiecuje, a użytkownik w ustawieniach rozumie skutek każdej opcji.

---

# Zadanie 06 — Szkoła: przełącznik widoczności, własna szkoła w profilu, brak zmiany

**Branch/worktree:** `rodo/06-szkola`
**Realizuje:** H4

## Decyzje właściciela

- Szkoły **nie można zmienić** po założeniu konta; zmiana wymaga nowego konta (zapisane w regulaminie w zadaniu 02).
- Nazwa szkoły przy ogłoszeniach dostaje **przełącznik widoczności, domyślnie `false`**.
- **Zalogowany użytkownik zawsze widzi własną szkołę** we własnym profilu — niezależnie od przełącznika — żeby wiedział, na którym koncie jest i czy nie założył konta ze złą szkołą.

## Zakres

1. Dodaj do modelu użytkownika flagę widoczności szkoły (blok na końcu klasy, komentarz `// RODO — zadanie 06`), wartość domyślna `false`. Migracja.
2. W `Ustawieniach profilu`, w sekcji danych widocznych dla innych, dodaj przełącznik „Pokaż moją szkołę przy moich ogłoszeniach". Opis pomocniczy ma mówić, kto zobaczy tę informację.
3. **Ukryj nazwę szkoły** przy karcie sprzedającego na stronie ogłoszenia oraz przy ogłoszeniach w profilu publicznym, gdy flaga jest `false`. Przejdź wszystkie miejsca, w których szkoła jest dziś renderowana — jest ich co najmniej dwa.
4. **Własny profil:** wyświetl nazwę szkoły zalogowanego użytkownika w jego własnym widoku profilu (`/Profile`) i w ustawieniach konta, jako pole tylko do odczytu, z adnotacją: szkoły nie można zmienić, a zmiana wymaga założenia nowego konta. Nie renderuj tam pola wyboru.
5. Upewnij się, że szkoły nie da się zmienić żadną ścieżką — sprawdź, czy formularz ustawień nie przyjmuje pola szkoły z żądania (podatność na podmianę wartości w `POST`).

## Zalecenie audytora — do decyzji właściciela, domyślnie **wykonaj**

Odesłanie użytkownika po nowe konto nie zamyka prawa do sprostowania danych (art. 16 RODO) — uczeń, który przy rejestracji kliknął złą pozycję na liście, traci ogłoszenia i historię, jeśli chce mieć poprawną szkołę. Tanie domknięcie: **umożliw administratorowi zmianę szkoły użytkownika w panelu, na zgłoszenie wysłane na `support@textbooker.pl`.** To kilkanaście linii w istniejącym panelu, a usuwa realną lukę.

Jeśli to zaimplementujesz — zrób to jako osobny, wyraźnie oznaczony fragment i opisz w raporcie, żeby właściciel mógł go łatwo wyciąć. Jeśli uznasz, że wykracza to poza zakres, pomiń i **zapisz to w sekcji „Do decyzji właściciela"**.

## Ukończone, gdy

Szkoła jest domyślnie niewidoczna dla innych, właściciel konta zawsze widzi swoją, a zmiana wartości przez formularz jest niemożliwa.

---

# Zadanie 07 — Ochrona przed nadużyciami: blokada logowania i limity ujawniania kontaktu

**Branch/worktree:** `rodo/07-limity`
**Realizuje:** H5

## Stan wyjściowy — potwierdzony testami

- 30 kolejnych żądań do endpointu ujawniającego dane kontaktowe: 11 odpowiedzi z danymi, **zero odrzuceń**. Identyczny wynik z konta zwykłego użytkownika i z konta administratora.
- 12 nieudanych prób logowania pod rząd: **żadnego spowolnienia ani blokady**.
- Identyfikatory ogłoszeń są sekwencyjne, więc całą bazę kontaktów da się pobrać pętlą.

## Decyzja właściciela

Limit **nie może przeszkadzać uczniom korzystającym z serwisu normalnie** — a normalne jest przeglądanie wielu ogłoszeń. Przy logowaniu: **ok. 10 nieudanych prób → 2 godziny przerwy.**

## Zakres

1. **Blokada logowania.** Skonfiguruj mechanizm blokady w ASP.NET Core Identity: 10 nieudanych prób, blokada 2 godziny, licznik zerowany po udanym logowaniu. Upewnij się, że ścieżka logowania faktycznie zlicza nieudane próby — sama konfiguracja nie wystarczy, jeśli kod logowania nie przekazuje odpowiedniej flagi.
   Komunikat dla zablokowanego użytkownika po polsku, bez zdradzania, czy konto istnieje. Napisz też, że można odzyskać dostęp przez reset hasła, jeśli to prawda w tej implementacji.
2. **Limit ujawniania kontaktu — hojny, ale skończony.** Dobierz progi tak, żeby uczeń przeglądający ogłoszenia przez godzinę nigdy ich nie dotknął, a pętla pobierająca całą bazę — tak. Punkt wyjścia do Twojej oceny: **rzędu 60 ujawnień na godzinę i 200 na dobę, licząc per konto**, przy czym **ponowne obejrzenie kontaktu do tego samego ogłoszenia nie zwiększa licznika** (uczeń wraca do ogłoszenia, które go interesuje — to nie jest nadużycie). Wartości zweryfikuj samodzielnie i **wyprowadź do konfiguracji**, żeby dało się je zmienić bez przebudowy.
   Po przekroczeniu limitu: uprzejmy komunikat po polsku, sugerujący kontakt z obsługą, a nie surowy błąd HTTP.
3. **Rejestrowanie ujawnień.** Zapisuj każde ujawnienie danych kontaktowych: kto, czyje dane, którego ogłoszenia dotyczyło, kiedy. To jest jednocześnie materiał do wykrywania scrapingu i ślad na wypadek incydentu. Wybierz najprostszą formę spójną z resztą projektu.
4. **Nie wprowadzaj CAPTCHA** ani żadnego rozwiązania zewnętrznego.

## Ukończone, gdy

Pętla po identyfikatorach ogłoszeń zostaje zatrzymana, przeglądanie kilkudziesięciu ogłoszeń w normalnym tempie nie jest niczym zakłócone, a dziesiąta nieudana próba logowania kończy się dwugodzinną blokadą. **Opisz w raporcie, jakie progi ostatecznie ustawiłeś i na jakiej podstawie.**

---

# Zadanie 08 — Ogłoszenia: ostrzeżenia o danych osobowych i moderacja treści

**Branch/worktree:** `rodo/08-tresci-ogloszen`
**Realizuje:** M6

## Problem

Formularz dodawania ogłoszenia przyjmuje dowolne zdjęcia i dowolny tekst, bez żadnego ostrzeżenia. W danych testowych są zarówno zdjęcia z wizerunkami osób, jak i opisy z treścią prywatną. Użytkownikami są w większości uczniowie.

## Zakres

1. **Ostrzeżenie przy zdjęciach.** Nad polem wyboru plików, widoczne bez najeżdżania kursorem, prostym językiem: żeby nie wysyłać zdjęć, na których widać osoby, twarze, adresy, plan lekcji, legitymację czy inne dane. Fotografować sam podręcznik.
2. **Ostrzeżenie przy opisie.** Krótka informacja, żeby nie wpisywać w opisie danych kontaktowych ani informacji o sobie — od kontaktu jest przycisk przy ogłoszeniu, a wszystko w opisie zobaczy każdy zalogowany użytkownik.
3. **Prosta kontrola treści opisu.** Wykryj w opisie wzorce wyglądające jak adres e-mail lub numer telefonu. Po wykryciu **nie blokuj publikacji** — pokaż ostrzeżenie i poproś o potwierdzenie. Uczeń może mieć powód, żeby coś takiego wpisać, a twarda blokada w serwisie dla nastolatków generuje obejścia i frustrację.
4. **Oznaczanie do przeglądu.** Ogłoszenia, w których wykryto takie wzorce, oznacz flagą do przejrzenia przez administratora. Wystarczy pole w modelu ogłoszenia i widoczne oznaczenie na liście w panelu — pełnej kolejki moderacyjnej nie budujesz.
5. Te same ostrzeżenia zastosuj w formularzu **edycji** ogłoszenia.

## Czego nie robisz

Nie wdrażasz rozpoznawania twarzy, moderacji zdjęć ani żadnej usługi zewnętrznej. Nie blokujesz publikacji.

## Ukończone, gdy

Użytkownik dodający ogłoszenie widzi oba ostrzeżenia, zanim cokolwiek wyśle, a opis z adresem e-mail lub numerem telefonu wymaga świadomego potwierdzenia i trafia do przejrzenia.

---

# Zadanie 09 — Panel administracyjny: dziennik działań i naprawa przycisku

**Branch/worktree:** `rodo/09-dziennik-admina`
**Realizuje:** M7

## Decyzja właściciela

Wdrażamy **wyłącznie dziennik działań administracyjnych** oraz ukrycie niedziałającego przycisku. **Nic więcej** — bez potwierdzania hasłem przy usuwaniu, bez usuwania miękkiego, bez maskowania adresów na liście. Nie rozszerzaj zakresu.

## Zakres

1. **Ukryj przycisk „Więcej…"** w wierszu użytkownika. Ma `href=""` i tylko przeładowuje stronę. Usuń go albo ukryj — wybierz, co czystsze, i opisz w raporcie.
2. **Dziennik działań administracyjnych.** Nowa encja rejestrująca co najmniej:
   - kto wykonał działanie (identyfikator i nazwa administratora),
   - jakiego rodzaju było to działanie,
   - kogo lub czego dotyczyło (identyfikator **i** nazwa, bo rekord może zostać usunięty),
   - kiedy,
   - parametry istotne dla działania — np. liczbę dni blokady.
   Wpis musi **przetrwać usunięcie obiektu, którego dotyczy** — nie wiąż go kluczem obcym z kaskadowym usuwaniem. Dziennika nie da się z panelu edytować ani kasować.
3. **Objęte działania:** blokada użytkownika, odblokowanie, usunięcie użytkownika, zmiana ról administratorów oraz operacje na szkołach, jeśli panel je udostępnia. Przejdź panel i wypisz w raporcie, co objąłeś, a czego nie i dlaczego.
4. **Podgląd dziennika** w panelu: prosta lista posortowana od najnowszych, z filtrem po administratorze i zakresie dat. Bez eksportu, bez stronicowania ponad to, co jest w projekcie standardem.
5. Zapis do dziennika ma następować **w tej samej transakcji** co działanie — nie może się zdarzyć, że użytkownik zniknie, a wpisu nie ma.

## Ukończone, gdy

Każde zablokowanie i usunięcie konta zostawia trwały ślad, czytelny w panelu, a niedziałający przycisk zniknął z interfejsu.

---

## Świadomie pominięte

Nie realizuj tych punktów — właściciel odłożył je świadomie. Nie proponuj ich, nie implementuj przy okazji, nie zgłaszaj jako braku:

| Punkt audytu | Decyzja |
|---|---|
| **M4** — pełniejszy zakres eksportu danych, nazwa szkoły zamiast identyfikatora | Identyfikator liczbowy zostaje. Odłożone. |
| **M8** — nagłówki bezpieczeństwa (HSTS, CSP, i pozostałe) | Odłożone. |
| **M10** — okresy przechowywania dla kont zablokowanych i zgłoszeń moderacyjnych | Odłożone. |
| **H6** — kasowanie zdjęć w Cloudflare R2 przy usuwaniu ogłoszenia i konta | Nierozstrzygnięte — czeka na wynik testu właściciela. |
| Pozycje organizacyjne (rejestr czynności, umowy powierzenia, procedura naruszeń, ocena skutków) | Poza kodem. |

Jeżeli podczas pracy natkniesz się na coś z tej listy i uznasz, że blokuje Ci zadanie — nie rozwiązuj tego samodzielnie, tylko opisz w sekcji „Do decyzji właściciela".

---

## Na koniec wszystkich zadań

Zbierz listę branchy wraz z lokalizacją worktree i ścieżką do raportu, w kolejności wykonania. Dopisz do niej:

- które zadania dotknęły plików spoza swojego zakresu (ryzyko konfliktu przy scalaniu),
- w jakiej kolejności warto scalać branche, żeby ograniczyć konflikty,
- gdzie zostały znaczniki `// TODO scalanie:` wymagające ręcznego domknięcia po scaleniu,
- przypomnienie, że po scaleniu wszystkich branchy migracje należy skasować i wygenerować od nowa jako jedną.
