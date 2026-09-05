# Scenariusze testowe dla Claude for Chrome — regresja, podstawowa funkcjonalność i RODO

Instrukcja przygotowana tak, żeby **Claude for Chrome robił cały test samodzielnie**. Jedyna rzecz,
której nie zrobi sam, to **zmiana konta (logowanie / wylogowanie)** — takich akcji odmawia. Tylko
w tych momentach zatrzymuje się i czeka na człowieka. **Wszystko inne — znalezienie adresu aplikacji,
klikanie, wypełnianie formularzy (łącznie z hasłami w formularzu rejestracji), czytanie, weryfikacja,
notowanie wyników, radzenie sobie z niespodziankami — robi sam, bez pytania.** Na końcu oddaje
**jeden raport z częściami: użytkownik, administrator i (jeśli doszło do skutku) niezalogowany**.

Ciężar testów leży na **zwykłym użytkowniku** (Faza 1) — to tam jest podstawowa funkcjonalność
i większość regresji. Administrator (Faza 2) ma tylko tyle testów, ile trzeba, żeby potwierdzić
skutki działań użytkownika i kluczowe funkcje panelu.

## Zasady dla Claude'a (przeczytaj przed startem)

1. **Zatrzymujesz się wyłącznie na zmianę konta.** Nie wpisujesz loginu ani hasła na stronie
   logowania, nie klikasz „Zaloguj" ani „Wyloguj". W wyznaczonych miejscach wypisujesz gotowy
   komunikat z bloku „PRZERWA NA PRZELOGOWANIE" i czekasz na potwierdzenie. **To jedyny powód,
   dla którego wolno Ci przerwać pracę i o coś poprosić** — z jednym wyjątkiem: pytanie o zgodę na
   opcjonalną Fazę 3 (zasada 8).
2. **Każde polecenie dla człowieka jest ostatnim zdaniem wiadomości**, w formacie
   `polecenie: zrób to i to`. Najpierw kontekst (co skończyłeś, czego potrzebujesz), a na samym
   końcu jedna linijka zaczynająca się od `polecenie:` — nic po niej. Człowiek ma widzieć od razu,
   co ma zrobić, bez czytania całości.
3. **Poza tym nie zadajesz żadnych pytań.** Adres aplikacji, wybór ogłoszenia do testu, treść
   testowego opisu, hasło w formularzu rejestracji (`TestPass123!`), interpretacja niejednoznacznego
   wyniku — rozstrzygasz sam i zapisujesz w raporcie, co i dlaczego wybrałeś.
4. **Adres aplikacji ustalasz sam.** Kolejno: sprawdź otwarte karty przeglądarki, potem spróbuj
   `https://localhost:5001`, `http://localhost:5000`, `https://localhost:7001`. Pierwszy adres, pod
   którym ładuje się Textbooker, jest adresem testowym — zanotuj go w raporcie. Dopiero gdyby żaden
   nie odpowiadał, napisz o tym w raporcie i zakończ (to awaria środowiska, nie pytanie do człowieka).
5. **Nie otwierasz okna prywatnego/incognito.** Testy „jak widzi to niezalogowany" robimy wyłącznie
   w opcjonalnej Fazie 3, kiedy człowiek świadomie wyloguje przeglądarkę.
6. **Nie przerywasz testów przy pierwszym błędzie.** Zapisujesz `BŁĄD`, robisz zrzut/cytat tego, co
   widzisz, i lecisz dalej. Wyjątek: jeśli aplikacja przestaje się ładować w ogóle.
7. Jeśli czegoś nie da się sprawdzić (brak danych, funkcja niedostępna) — status `NIE SPRAWDZONO`
   plus jedno zdanie dlaczego. Nie zgadujesz wyniku i nie pytasz człowieka, co z tym zrobić.
8. **Faza niezalogowana jest ostatnia i opcjonalna.** Wykonujesz ją tylko wtedy, gdy człowiek
   wyraźnie się zgodzi, kiedy go o to zapytasz po ostatnim teście administratora. Jeśli odmówi albo
   nie odpowie — wszystkie testy `W1`–`W9` dostają status `NIE SPRAWDZONO` z uzasadnieniem „faza
   niezalogowana pominięta na życzenie człowieka" i od razu oddajesz raport. **Pytasz o to jeden
   raz, nie ponawiasz.**
9. **Testy oznaczone „(łańcuch)" mają następstwo w innym teście** — zawsze zapisuj wskazane w nich
   URL-e i wartości, bo później będą potrzebne.

## Kolejność faz — i dlaczego taka

**Kolejność: zwykły użytkownik → administrator → (opcjonalnie, na końcu) niezalogowany.**

- Testy admina (`A2` oznaczenie ogłoszenia „do przejrzenia", `A4`/`A5` dziennik działań) weryfikują
  **skutki** rzeczy, które robi zwykły użytkownik — dlatego użytkownik zawsze idzie przed adminem.
- Testy niezalogowanego (`W1`–`W9`) nie są warunkiem żadnego innego testu, więc stoją na końcu.
  Jeśli człowiek nie chce trzeciego przelogowania, **komplet testów użytkownika i administratora
  jest już zrobiony**, a raport i tak da się oddać.
- Przy tej kolejności potrzebne są **dwie przerwy na przelogowanie** (`u1`, potem `a1`) plus trzecia
  **tylko jeśli** człowiek zgodzi się na Fazę 3.

## Środowisko testowe (dane zaseedowane)

- **Administrator:** `a1` / `TestPass123!`
- **Zwykli użytkownicy:** `u1`–`u6` / `TestPass123!`
- **Szkoły i przypisanie kont** (ważne dla testów izolacji szkolnej):
  - `Hogwort` → `u1`, `u4`, `a1` oraz konta generowane z prefiksem `r1_`
  - `Technikum Pod Patronatem Przypadkowego Gościa z Discorda` → `u2`, `u5`, konta `r2_`
  - `Uniwersytet Bestroskiego Zycia…` → `u3`, `u6`, konta `r3_`
- **Wyłączone funkcje (flagi w konfiguracji):** wiadomości/czat (`MessagesEnabled`) oraz zdjęcia
  profilowe (`ProfilePhotosEnabled`). Brak pozycji „Wiadomości" w menu i domyślny awatar przy
  sprzedającym to **stan oczekiwany**, nie błąd.

### Zdjęcia w formularzu ogłoszenia — przeczytaj przed `U11`
Dodanie nowego ogłoszenia **wymaga co najmniej jednego zdjęcia** (`.jpg`, `.jpeg`, `.png`, do 5 MB,
maks. 6 plików). Zanim zaczniesz `U11`, przygotuj sobie plik sam: otwórz dowolne ogłoszenie, zapisz
okładkę na dysk (menu kontekstowe → „Zapisz obraz jako…") albo użyj dowolnego obrazu, który już masz.
Jeśli po realnej próbie okaże się, że nie potrafisz obsłużyć wyboru pliku, oznacz `U11` jako
`NIE SPRAWDZONO` z uzasadnieniem „nie udało się wgrać zdjęcia", **wykonaj wszystkie pozostałe testy**
i pamiętaj, że wtedy `U12`, `U13`, `U14` i `A2` trzeba wykonać na innym ogłoszeniu albo również
oznaczyć jako `NIE SPRAWDZONO`. Nie proś człowieka o pomoc ze zdjęciem.

---

## Faza 1 — ZWYKŁY UŻYTKOWNIK (`u1`)

### Start — robisz sam
Ustal adres aplikacji według zasady 4, otwórz stronę główną i **sam sprawdź w nawigacji, kto jest
zalogowany**. Jeśli to już konto `u1` — zaczynaj od `U1`, nie pisz nic do człowieka.

### PRZERWA NA PRZELOGOWANIE #1 (jeśli w przeglądarce nie ma konta `u1`)
Piszesz i czekasz:

> Adres testowy, którego używam: `<adres>`. W przeglądarce jest `<nazwa konta / brak zalogowanego>`,
> a testy zaczynam od zwykłego użytkownika. Sam się nie loguję ani nie wylogowuję — poproszę Cię
> jeszcze raz w trakcie. Resztę robię sam.
>
> polecenie: zaloguj się jako `u1` / `TestPass123!` i napisz „ok".

### U1. Nawigacja i menu konta po zalogowaniu (regresja)
Rozwiń menu konta w nagłówku. **Oczekiwane:** widać „Witaj u1!", a w rozwijanym menu: `Profil`,
`Ulubione`, `Ustawienia`, `Wyloguj`. Obok menu jest przycisk `Dodaj ogłoszenie`. **Nie ma** pozycji
`Panel administracyjny` (to konto nie jest adminem) ani `Wiadomości` (funkcja wyłączona flagą).

### U2. Strona główna po zalogowaniu (regresja)
Otwórz stronę główną. **Oczekiwane:** ładuje się sekcja z wyróżnionymi ogłoszeniami (zdjęcia + ceny),
lista najnowszych ogłoszeń, lista przedmiotów i stopka. Żadnych pustych kafelków, brakujących
obrazków ani błędu 500.

### U3. Lista ogłoszeń i doładowywanie przy przewijaniu (regresja)
Wejdź na `/Browse`. **Oczekiwane:** ładuje się siatka kafelków; każdy kafelek ma okładkę, tytuł, cenę
w złotówkach i tagi (przedmiot, klasa, poziom). Przewiń stronę do końca — **kolejna porcja ogłoszeń
doładowuje się sama** (strona ma 25 pozycji, doładowanie jest automatyczne, bez klikania „więcej").
Zanotuj, czy doładowanie zadziałało za pierwszym przewinięciem.

### U4. Filtry: przedmiot, klasa, poziom (regresja)
Na `/Browse` ustaw kolejno filtr przedmiotu, potem klasy, potem poziomu. **Oczekiwane:** lista
przeładowuje się po każdej zmianie (bez przeładowania całej strony), a widoczne kafelki mają tag
zgodny z wybranym filtrem. Kliknij tag przedmiotu **na samym kafelku** — powinien ustawić ten sam
filtr. Na końcu kliknij przycisk czyszczenia filtrów. **Oczekiwane:** wszystkie pola wracają do
stanu pustego i lista pokazuje znowu pełny zestaw ogłoszeń.

### U5. Filtr ceny, w tym przypadek brzegowy (regresja)
Ustaw `Cena min.` i `Cena max.` tak, żeby zostało kilka ogłoszeń, i sprawdź, że żadna widoczna cena
nie wypada poza zakres. Potem ustaw **min większe niż max** (np. min `999`, max `1`).
**Oczekiwane:** pusta lista albo komunikat o braku wyników — **nie** błąd aplikacji i nie lista
zignorowana w całości.

### U6. Wyszukiwarka (regresja)
Wpisz w pole wyszukiwania fragment tytułu jednego z widocznych ogłoszeń. **Oczekiwane:** lista
zawęża się do pasujących pozycji w trakcie pisania (bez klikania „szukaj"). Wpisz następnie ciąg,
który na pewno nie pasuje (np. `qqqzzz`). **Oczekiwane:** brak wyników i brak błędu.

### U7. Izolacja szkolna — użytkownik widzi tylko swoją szkołę (kluczowa funkcja, łańcuch)
`u1` należy do szkoły `Hogwort`. Otwórz z `/Browse` **pięć różnych ogłoszeń** i za każdym razem
sprawdź w sekcji „Sprzedaje" nazwę sprzedającego. **Oczekiwane:** wszystkie nazwy to `u1`, `u4`, `a1`
albo konta z prefiksem `r1_` — **nigdzie** nie pojawia się `r2_` ani `r3_` (to konta z innych szkół).
**Zapisz w raporcie pięć zaobserwowanych nazw sprzedających** — w `W5` sprawdzisz, że niezalogowany
widzi także konta `r2_`/`r3_`. **Zapisz też URL jednego cudzego ogłoszenia** — przyda się w `U9`,
`U19`, `W3` i `W4`.

### U8. Karta ogłoszenia — komplet informacji i galeria (regresja)
Na otwartym cudzym ogłoszeniu sprawdź, że są: tytuł, cena, lista `Klasa / Przedmiot / Zakres / Stan`,
data dodania w formacie opisowym (np. „dzisiaj o 14:05"), sekcja `Opis` i sekcja `Sprzedaje` z nazwą
użytkownika oraz przyciskiem `Więcej od tego użytkownika`. Jeśli ogłoszenie ma miniatury zdjęć —
kliknij drugą miniaturę. **Oczekiwane:** główne zdjęcie się podmienia, klikniętą miniaturę widać jako
aktywną.

### U9. Ulubione — dodanie i usunięcie (regresja)
Na cudzym ogłoszeniu z `U7` kliknij przycisk ulubionych (serduszko). **Oczekiwane:** przycisk zmienia
stan od razu, bez przeładowania strony. Wejdź w `Ulubione` w menu konta — ogłoszenie jest na liście.
Usuń je z ulubionych z poziomu kafelka i odśwież `Ulubione`. **Oczekiwane:** lista znowu jest bez
tego ogłoszenia. Na koniec **dodaj je ponownie**, żeby `U19` (pobranie danych) miało co pokazać
w sekcji `ulubione`.

### U10. Profil innego użytkownika (regresja)
Na cudzym ogłoszeniu kliknij `Więcej od tego użytkownika`. **Oczekiwane:** otwiera się profil
(`/Profile/<id>`) z nazwą użytkownika i siatką jego ogłoszeń; kafelki działają tak samo jak na
`/Browse`. Następnie ręcznie otwórz `/Profile/Favorites/<ten sam id>`. **Oczekiwane:** strona nie
pokazuje cudzych ulubionych — ma być `404`/„nie znaleziono", bo domyślnie ulubione nie są publiczne.

### U11. Dodanie ogłoszenia + ostrzeżenie o danych osobowych w opisie (kluczowy test RODO, łańcuch)
Kliknij `Dodaj ogłoszenie` i wypełnij formularz **sam**: tytuł wybierz z listy, przedmiot/klasę/poziom
ustaw dowolnie, stan opisz krótko (np. `dobry`), cenę ustaw na np. `12,50`, w opisie wpisz
`Test RODO <data>, kontakt: test@przyklad.pl`, dołącz przygotowane wcześniej zdjęcie.
**Oczekiwane:** po wysłaniu pojawia się ostrzeżenie, że opis wygląda na zawierający dane kontaktowe,
i formularz **wymaga zaznaczenia potwierdzenia**, zanim opublikuje ogłoszenie (publikacja nie jest
zablokowana na stałe). Zaznacz potwierdzenie i wyślij ponownie. **Oczekiwane:** ogłoszenie powstaje.
**Zapisz jego tytuł i URL** — potrzebne w `U12`, `U13`, `U14`, `U16`, `A2` i `W6`.

### U12. Walidacja formularza ogłoszenia (regresja)
Otwórz formularz dodawania jeszcze raz i **wyślij go pusty**. **Oczekiwane:** komunikaty przy polach
wymaganych (tytuł, przedmiot, klasa, poziom, stan, cena) i informacja, że trzeba przesłać
przynajmniej jedno zdjęcie. Potem sprawdź trzy przypadki brzegowe: cena `0` → `Cena musi być większa
od zera`; opis stanu dłuższy niż 40 znaków → komunikat o limicie; próba dołączenia pliku innego niż
obraz (np. `.txt`, jeśli masz taki pod ręką) → komunikat o niedozwolonym rozszerzeniu.
**Nie publikuj** tego ogłoszenia — po sprawdzeniu walidacji opuść formularz.

### U13. Edycja własnego ogłoszenia (regresja)
Otwórz ogłoszenie z `U11` i kliknij `Edytuj ogłoszenie`. Zmień cenę i dopisz zdanie do opisu, zapisz
**bez dodawania nowych zdjęć**. **Oczekiwane:** zapis się udaje mimo braku nowego zdjęcia (przy
edycji zdjęcie nie jest wymagane), na karcie widać nową cenę i nową treść opisu, a pod datą dodania
pojawia się linia `Edytowane: …`. Zdjęcia z pierwotnego ogłoszenia nie znikają.

### U14. Rezerwacja własnego ogłoszenia (regresja)
Na ogłoszeniu z `U11` zaznacz `Oznacz jako zarezerwowane`. **Oczekiwane:** po odświeżeniu na karcie
jest komunikat `Ten przedmiot jest obecnie zarezerwowany`, a kafelek na `/Browse` lub w profilu jest
wyróżniony jako zarezerwowany. **Odznacz rezerwację z powrotem** i potwierdź, że komunikat znika.

### U15. Licznik wyświetleń widzi tylko właściciel (regresja)
Na własnym ogłoszeniu z `U11` sprawdź, że pod datą jest `Wyświetleń: <liczba>`. Następnie otwórz
cudze ogłoszenie z `U7`. **Oczekiwane:** przy cudzym ogłoszeniu **nie ma** licznika wyświetleń —
to informacja wyłącznie dla właściciela.

### U16. Widoczność szkoły — włączenie zgody (kluczowy test RODO, część 1, łańcuch)
Wejdź w `Ustawienia` i włącz przełącznik `Pokaż moją szkołę przy moich ogłoszeniach` (domyślnie
wyłączony), zapisz. **Oczekiwane:** komunikat o zaktualizowaniu profilu. Otwórz własne ogłoszenie
z `U11`. **Oczekiwane:** przy sprzedającym widać nazwę szkoły (`Hogwort`), a nazwa szkoły pojawia się
też na kafelku tego ogłoszenia na liście. **Zostaw przełącznik włączony** — jest potrzebny w `W6`.

### U17. Ustawienia kontaktu — walidacja form kontaktu (regresja)
Wciąż w `Ustawieniach` sprawdź walidację, **nie zostawiając konta bez kontaktu**:
1. Odznacz wszystkie formy kontaktu (e-mail, telefon, WhatsApp, Messenger, Instagram) i zapisz.
   **Oczekiwane:** błąd `Musisz wybrać przynajmniej jedną formę kontaktu`, ustawienia się nie zapisują.
2. Zaznacz `WhatsApp` przy pustym numerze telefonu i zapisz. **Oczekiwane:** komunikat, że aby wybrać
   WhatsApp, trzeba podać numer telefonu.
3. Zaznacz `Messenger` z pustą nazwą użytkownika. **Oczekiwane:** analogiczny komunikat.
4. Na koniec **przywróć stan działający** — zaznacz `Pokaż mój e-mail jako dostępną formę kontaktu`
   i zapisz, żeby `U19` i testy kontaktu miały sens. Potwierdź komunikat o zapisaniu profilu.

Sprawdź też, że **szkoły nie da się zmienić z tego formularza** — nazwa szkoły jest wyświetlona,
ale nie jako pole do edycji.

### U18. Zmiana hasła — sama walidacja, bez zmiany hasła (regresja)
Wejdź w `Ustawienia` → `Hasło`. Wpisz **błędne** obecne hasło i dowolne nowe, zapisz.
**Oczekiwane:** komunikat o nieprawidłowym haśle, hasło konta **nie** zostaje zmienione.
**Nie wykonuj poprawnej zmiany hasła** — konto `u1` musi zachować hasło `TestPass123!`.

### U19. Dane kontaktowe sprzedającego — tylko przy aktywnym ogłoszeniu (kluczowy test RODO)
Otwórz cudze ogłoszenie z `U7` i kliknij przycisk `Zapytaj o przedmiot`. **Oczekiwane:** pokazują się
dane kontaktowe sprzedającego (te formy, które ten użytkownik ma włączone). Jeśli uda Ci się znaleźć
profil użytkownika **bez żadnych aktywnych ogłoszeń** — sprawdź, że zamiast danych jest komunikat,
że użytkownik nie ma obecnie aktywnych ogłoszeń, więc dane nie są udostępniane. Jeśli takiego profilu
nie ma — status `NIE SPRAWDZONO`; **nie kombinuj z usuwaniem cudzych ogłoszeń**.

Sprawdź też przypadek własny: na **swoim** ogłoszeniu z `U11` przycisku `Zapytaj o przedmiot` nie ma
(zamiast niego jest `Edytuj ogłoszenie` i rezerwacja).

### U20. Pobranie danych osobowych (regresja kluczowej funkcji RODO)
`Ustawienia` → `Dane osobowe` → `Pobierz`. **Oczekiwane:** pobiera się plik JSON z sekcjami `konto`,
`ogloszenia`, `ulubione`. Sekcja `ogloszenia` zawiera ogłoszenie z `U11`, a `ulubione` — ogłoszenie
dodane w `U9`. Zajrzyj do pliku i zanotuj, czy któraś sekcja jest pusta mimo że powinna mieć dane.

### U21. Strona błędu dla nieistniejącego ogłoszenia (regresja)
Otwórz ręcznie `/Book/999999`. **Oczekiwane:** przyjazna strona „nie znaleziono" (404), a nie surowy
błąd serwera ani pusta strona.

---

## Faza 2 — ADMINISTRATOR (`a1`)

### PRZERWA NA PRZELOGOWANIE #2
Claude pisze i czeka:

> Faza użytkownika skończona (U1–U21). Teraz potrzebuję konta administratora.
>
> polecenie: zaloguj się jako `a1` / `TestPass123!` i napisz „ok".

### A1. Dostęp do panelu i nawigacja (regresja)
Sprawdź, że w menu konta pojawiła się pozycja `Panel administracyjny`, i wejdź na `/Admin`.
**Oczekiwane:** panel się ładuje, w nawigacji są sekcje: Podsumowanie / Użytkownicy /
Administratorzy / Szkoły / Ogłoszenia / Dziennik działań, a strona Podsumowania pokazuje liczby
(użytkownicy, ogłoszenia, szkoły) zamiast pustych miejsc.

### A2. Ogłoszenie oznaczone „do przejrzenia" (sprzężone z U11)
Wejdź na `/Admin/Items` i znajdź ogłoszenie dodane w `U11` (po tytule). **Oczekiwane:** w kolumnie
`Status` jest `⚠ Do przejrzenia`, bo opis zawierał adres e-mail. Sprawdź dla kontrastu, że zwykłe
ogłoszenia mają w tej kolumnie `—`. Kliknij tytuł ogłoszenia — link prowadzi na jego kartę.

### A3. Lista użytkowników: wyszukiwarka i brak martwego przycisku (regresja)
Wejdź na `/Admin/Users`. **Oczekiwane:** tabela użytkowników się ładuje, a wyszukiwarka po wpisaniu
`u1` zawęża listę do pasujących kont. Przycisku `Więcej…` przy użytkownikach **nie ma** (wcześniej
istniał, ale tylko przeładowywał stronę bez efektu).

### A4. Blokada/odblokowanie użytkownika i dziennik działań (najważniejszy test tej fazy)
Na `/Admin/Users` zablokuj użytkownika **innego niż `a1` i innego niż `u1`** (np. `u2`) na 1 dzień,
a potem od razu go odblokuj. Wejdź na `/Admin/AuditLog`. **Oczekiwane:** dwa nowe wpisy (blokada
i odblokowanie) z nazwą administratora (`a1`), nazwą celu (`u2`) i znacznikiem czasu, widoczne
natychmiast po akcji — wpis ma powstawać w tej samej transakcji co akcja.
**Upewnij się, że `u2` został odblokowany**, zanim przejdziesz dalej.

### A5. Dziennik nie gubi kontekstu (regresja)
Na `/Admin/AuditLog` sprawdź, że każdy wpis ma komplet: kto / co / na kim / kiedy — brak pustych
kolumn w nowych wpisach z `A4`.

### A6. Izolacja szkolna obowiązuje też konto administratora (regresja)
`a1` należy do szkoły `Hogwort`. Na `/Admin/Items` znajdź ogłoszenie, którego sprzedający ma nazwę
zaczynającą się od `r2_` lub `r3_` (inna szkoła) i kliknij jego tytuł. **Oczekiwane:** publiczna
karta ogłoszenia **nie otwiera się** — `404`/„nie znaleziono", bo izolacja szkolna działa dla
każdego zalogowanego konta, również administracyjnego. Panel admina nadal pokazuje to ogłoszenie
w tabeli. **Zapisz URL tego ogłoszenia** — w `W5` sprawdzisz, że niezalogowany je widzi.

### A7. Lista szkół (regresja)
Wejdź na `/Admin/Schools`. **Oczekiwane:** widać trzy zaseedowane szkoły z domenami e-mail, a filtr
pokazywania nieaktywnych działa. **Nie dodawaj, nie edytuj i nie dezaktywuj żadnej szkoły** —
zmiana szkoły przestawia widoczność ogłoszeń w całym środowisku testowym.

---

## Faza 3 (OPCJONALNA) — NIEZALOGOWANY

**Tej fazy nie zaczynasz z automatu.** Po `A7` masz już komplet wyników użytkownika i administratora.
Pytasz człowieka jeden raz, czy chce jeszcze fazę niezalogowaną:

> Fazy użytkownika i administratora skończone (U1–U21, A1–A7) — mam komplet wyników i mogę już oddać
> raport. Zostały testy dla niezalogowanego (W1–W9: stopka, przeglądanie bez logowania, ogłoszenia ze
> wszystkich szkół, ukryta szkoła i dane kontaktowe, rejestracja bez zgód, dostęp do stron chronionych).
> Wymagają jeszcze jednego wylogowania. Jeśli nie chcesz — napisz „pomiń", oznaczę je jako
> NIE SPRAWDZONO i od razu oddam raport.
>
> polecenie: jeśli mam zrobić testy dla niezalogowanego, wyloguj się i napisz „ok"; jeśli nie — napisz „pomiń".

Jeśli człowiek odmówi (albo poprosi o raport bez tej fazy) — **nie ponawiasz pytania**, wpisujesz
`W1`–`W9` jako `NIE SPRAWDZONO` z uzasadnieniem i przechodzisz do raportu.

### W1. Stopka — jeden adres kontaktowy i komplet linków (regresja)
Na stronie głównej sprawdź stopkę: „Regulamin", „Polityka prywatności", „Polityka prywatności
w skrócie", „Kontakt" → `support@textbooker.pl`. **Oczekiwane:** cztery linki działają, prowadzą do
właściwych stron, adres kontaktowy wszędzie identyczny.

### W2. Przeglądanie bez logowania (regresja)
Wejdź na stronę główną i `/Browse`. **Oczekiwane:** strona główna i lista ogłoszeń ładują się dla
niezalogowanego, filtry i wyszukiwarka działają, a w nagłówku są `Zaloguj się` i `Zarejestruj się`
zamiast menu konta.

### W3. Szkoła niewidoczna dla niezalogowanego (regresja)
Otwórz cudze ogłoszenie, którego URL zapisałeś w `U7`. **Oczekiwane:** przy sprzedającym **nie ma**
nazwy szkoły — niezalogowany nigdy jej nie widzi. Nazwy szkół nie ma też na kafelkach na `/Browse`.

### W4. Dane kontaktowe sprzedającego dla niezalogowanego (kluczowy test RODO)
Na tym samym ogłoszeniu kliknij `Zapytaj o przedmiot`. **Oczekiwane:** dane kontaktowe **nie**
pokazują się bez zalogowania — aplikacja przenosi na stronę logowania (odwrotnie niż w `U19`).
Nie loguj się — zanotuj i wróć.

### W5. Ogłoszenia ze wszystkich szkół dla niezalogowanego (para do U7)
Wejdź na `/Browse` i otwórz kilka ogłoszeń, sprawdzając nazwy sprzedających; otwórz też URL zapisany
w `A6`. **Oczekiwane:** niezalogowany widzi także sprzedających `r2_` i `r3_` (inne szkoły), a
ogłoszenie z `A6` — niedostępne dla zalogowanego `a1` — **otwiera się normalnie**. To potwierdza, że
izolacja szkolna dotyczy tylko kont z przypisaną szkołą.

### W6. Szkoła niewidoczna mimo włączonej zgody (kluczowy test RODO, część 2)
Otwórz URL ogłoszenia `u1` zapisany w `U11`/`U16`. **Oczekiwane:** nazwa szkoły **nie** jest
widoczna, mimo że w `U16` przełącznik był włączony i szkoła była widoczna po zalogowaniu. Zgoda
działa tylko wobec zalogowanych użytkowników.

### W7. Strony wymagające logowania są chronione (regresja)
Otwórz kolejno `/Add`, `/Profile`, `/Identity/Account/Manage`. **Oczekiwane:** każda z nich przenosi
na stronę logowania (albo pokazuje „brak dostępu"), a nie renderuje formularza dla niezalogowanego.

### W8. Rejestracja — informacje, linki i auto-przypisanie szkoły (regresja, bez wysyłania)
Wejdź na stronę rejestracji. Sprawdź, że link „regulamin" przy checkboxie otwiera treść regulaminu
(nowa karta) i że nad przyciskiem `Zarejestruj` jest informacja o administratorze danych z linkami do
„Polityki prywatności w skrócie" i „Pełnej polityki prywatności". Następnie wpisz w pole e-mail adres
z domeny szkolnej, np. `test+rodo@hogwart.edu.pl`, i przejdź do następnego pola.
**Oczekiwane:** lista szkół sama ustawia się na `Hogwort` (dopasowanie po domenie). Wpisz potem adres
z nieznanej domeny (np. `test+rodo@przyklad.pl`) — **oczekiwane:** można wybrać szkołę ręcznie.
Nie wysyłaj formularza w tym teście.

### W9. Rejestracja bez akceptacji regulaminu — musi się nie udać (kluczowy test RODO)
Na tej samej stronie wypełnij **sam** wszystkie pola poprawnie — e-mail testowy
(`test+rodo@przyklad.pl`), hasło `TestPass123!` w obu polach — ale **nie zaznaczaj** checkboxa
akceptacji regulaminu ani oświadczenia o wieku. Kliknij `Zarejestruj`. **Oczekiwane:** rejestracja
się nie udaje, przy **obu** checkboxach jest błąd walidacji, konto nie powstaje. To naprawiona
luka — wcześniej dało się to ominąć.

> To jest formularz rejestracji, a nie logowanie — wpisujesz w nim wszystko sam, łącznie z hasłem.
> Wysyłka i tak ma się nie udać, więc żadne konto nie powstanie.

**Testu pozytywnego (rejestracja z zaznaczonymi zgodami) nie robimy** — tworzyłby realne konto
i mógłby automatycznie zalogować przeglądarkę.

---

## Raport końcowy

Po `A7` (jeśli faza niezalogowana została pominięta) albo po `W9` Claude oddaje jeden raport, bez
zadawania dodatkowych pytań. Format:

```
Adres testowy: ...

## Raport — użytkownik (faza 1)

| ID | Co sprawdzano | Wynik | Dowód / uwagi |
|----|---------------|-------|----------------|
| U1 | Menu konta: pozycje, brak panelu admina i wiadomości | OK / BŁĄD / NIE SPRAWDZONO | ... |
| U2 | Strona główna po zalogowaniu | | |
| U3 | Lista ogłoszeń + doładowywanie przy przewijaniu | | |
| U4 | Filtry przedmiot/klasa/poziom + czyszczenie filtrów | | |
| U5 | Filtr ceny, w tym min > max | | |
| U6 | Wyszukiwarka (trafienie i brak trafień) | | |
| U7 | Izolacja szkolna — tylko sprzedający z Hogwortu | | |
| U8 | Karta ogłoszenia: komplet danych i galeria zdjęć | | |
| U9 | Ulubione: dodanie, lista, usunięcie | | |
| U10 | Profil innego użytkownika + cudze ulubione niedostępne | | |
| U11 | Dodanie ogłoszenia + ostrzeżenie o danych w opisie | | |
| U12 | Walidacja formularza ogłoszenia (pola, cena, stan, plik) | | |
| U13 | Edycja ogłoszenia bez dodawania zdjęć + data edycji | | |
| U14 | Rezerwacja i jej cofnięcie | | |
| U15 | Licznik wyświetleń tylko dla właściciela | | |
| U16 | Szkoła widoczna po włączeniu zgody (zalogowany) | | |
| U17 | Walidacja form kontaktu + szkoły nie da się zmienić | | |
| U18 | Zmiana hasła: błędne obecne hasło odrzucone | | |
| U19 | Kontakt sprzedającego tylko przy aktywnym ogłoszeniu | | |
| U20 | Pobranie danych osobowych (JSON) | | |
| U21 | 404 dla nieistniejącego ogłoszenia | | |

## Raport — administrator (faza 2)

| ID | Co sprawdzano | Wynik | Dowód / uwagi |
|----|---------------|-------|----------------|
| A1 | Dostęp do panelu, nawigacja i podsumowanie | | |
| A2 | Ogłoszenie z U11 oznaczone „Do przejrzenia" | | |
| A3 | Wyszukiwarka użytkowników, brak przycisku „Więcej…" | | |
| A4 | Blokada/odblokowanie + wpisy w dzienniku | | |
| A5 | Komplet danych w nowych wpisach dziennika | | |
| A6 | Izolacja szkolna dotyczy też konta admina | | |
| A7 | Lista szkół i filtr nieaktywnych | | |

## Raport — niezalogowany (faza 3, opcjonalna)
(jeśli człowiek nie zgodził się na tę fazę: jedno zdanie „pominięta na życzenie człowieka"
i wszystkie pozycje jako NIE SPRAWDZONO)

| ID | Co sprawdzano | Wynik | Dowód / uwagi |
|----|---------------|-------|----------------|
| W1 | Stopka: 4 linki, support@textbooker.pl | | |
| W2 | Strona główna i Browse bez logowania | | |
| W3 | Szkoła niewidoczna dla niezalogowanego | | |
| W4 | Kontakt sprzedającego niedostępny bez logowania | | |
| W5 | Niezalogowany widzi ogłoszenia ze wszystkich szkół | | |
| W6 | Szkoła niewidoczna mimo włączonej zgody | | |
| W7 | /Add, /Profile, Ustawienia chronione logowaniem | | |
| W8 | Rejestracja: linki, info o administratorze, auto-szkoła | | |
| W9 | Rejestracja bez zgód odrzucona | | |

## Podsumowanie
- Błędy do naprawy: ...
- Nie sprawdzono (i dlaczego): ...
- Decyzje podjęte samodzielnie: (który adres, które cudze ogłoszenie, jakie dane testowe, skąd zdjęcie)
- Stan po testach: (czy `u2` odblokowany, jakie ogłoszenie dodano i czy rezerwacja zdjęta,
  czy przełącznik szkoły w `u1` zostawiono włączony, jakie ustawienia kontaktu ma `u1`)
```

---

## Czego celowo NIE robimy

- **Claude sam się nie loguje, nie wylogowuje i nie rejestruje realnego konta.** Zmiany konta robi
  wyłącznie człowiek, w dwóch wyznaczonych przerwach (`u1`, `a1`) plus trzeciej, tylko jeśli zgodzi
  się na opcjonalną Fazę 3. Jedyne dodatkowe pytanie do człowieka to zgoda na Fazę 3. Każda taka
  prośba kończy się osobną, ostatnią linijką `polecenie: ...`.
- **Nie zaczynamy od niezalogowanego.** Faza niezalogowana jest ostatnia i tylko za zgodą człowieka —
  testy użytkownika i administratora mają być zrobione, zanim poprosimy o cokolwiek ekstra.
- **Nie prosimy człowieka o pomoc ze zdjęciem do ogłoszenia** — plik zdobywamy sami albo oznaczamy
  test jako `NIE SPRAWDZONO`.
- **Nie otwieramy okna prywatnego/incognito.**
- **Nie zmieniamy hasła ani adresu e-mail kont testowych** — sprawdzamy tylko walidację przy błędnych
  danych. Konta `u1`–`u6` i `a1` muszą zostać przy haśle `TestPass123!`.
- **Nie testujemy blokady logowania po 10 nieudanych próbach.** Jedna próba z błędnym hasłem
  wystarczy, żeby zobaczyć, że komunikat jest ogólny; faktyczna blokada wyłącza konto na 2 godziny.
- **Nie wyczerpujemy limitu ujawnień danych kontaktowych** (100/godzinę).
- **Nie usuwamy żadnego konta testowego** (`/Identity/Account/Manage/DeletePersonalData`) — akcja
  nieodwracalna.
- **Nie usuwamy cudzych ogłoszeń** ani nie kasujemy ogłoszenia dodanego w `U11` — admin sprawdza je
  w `A2`.
- **Nie dodajemy, nie edytujemy i nie dezaktywujemy szkół ani administratorów** — te zmiany
  przestawiają widoczność ogłoszeń w całym środowisku testowym.
- **Nie zostawiamy `u2` zablokowanego** po `A4` ani ogłoszenia z `U11` zarezerwowanego po `U14`.
