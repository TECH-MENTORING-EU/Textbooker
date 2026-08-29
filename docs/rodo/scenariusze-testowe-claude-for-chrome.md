# Scenariusze testowe dla Claude for Chrome — po scaleniu poprawek RODO

Instrukcja do wykonania w przeglądarce, krok po kroku. Celowo krótka — 4 scenariusze dla
administratora, 7 dla zwykłego użytkownika. Częśeć scenariuszy to uniwersalna regresja (działałaby
niezależnie od zmian RODO), część celowo sprawdza konkretne nowe zachowania wprowadzone w tej
serii zmian — każdy scenariusz mówi, co dokładnie sprawdza i dlaczego.

## Zanim zaczniesz

- Aplikacja musi już działać lokalnie (`dotnet run` uruchomione przez użytkownika) — **zapytaj
  użytkownika o dokładny adres URL**, zanim zaczniesz (domyślnie może to być
  `https://localhost:5001` albo inny port pokazany w konsoli — nie zgaduj).
- Dane logowania (środowisko deweloperskie, dane zaseedowane automatycznie):
  - **Administrator:** login `a1`, hasło `TestPass123!`
  - **Zwykli użytkownicy:** loginy `u1`–`u6`, hasło `TestPass123!` (wszystkie to samo hasło)
- **Zaczynamy od scenariuszy administratora.** Po ich ukończeniu — zanim przelogujesz się na
  zwykłego użytkownika — **zatrzymaj się i zapytaj użytkownika, czy przejść dalej.** Nie loguj się
  na konto zwykłego użytkownika bez tego potwierdzenia.
- Zapisuj na bieżąco: co sprawdzałeś, co zobaczyłeś, czy zgadza się z oczekiwanym wynikiem. Jeśli
  coś nie zgadza się z opisem — zatrzymaj się i zgłoś to zamiast kontynuować dalsze scenariusze.

---

## Część 1 — Administrator (konto: `a1` / `TstPass123!`)

### A1. Podstawowy dostęp do panelu (regresja)
Zaloguj się jako `a1`. Wejdź na `/Admin`. **Oczekiwane:** panel się ładuje, widać sekcje
Podsumowanie / Użytkownicy / Administratorzy / Szkoły / Ogłoszenia / Dziennik działań w nawigacji
panelu.

### A2. Lista ogłoszeń i oznaczenie do przeglądu (nowość)
Wejdź na `/Admin/Items`. **Oczekiwane:** lista ogłoszeń się ładuje, jest kolumna pokazująca, czy
ogłoszenie jest oznaczone „do przejrzenia" (może być pusta, jeśli nikt jeszcze nie dodał
podejrzanego opisu — to też prawidłowy wynik na tym etapie, wrócimy do tego w scenariuszu U3).

### A3. Lista użytkowników — usunięty martwy przycisk (regresja)
Wejdź na `/Admin/Users`. **Oczekiwane:** przycisk „Więcej…" przy użytkownikach **nie istnieje**
(wcześniej istniał, ale tylko przeładowywał stronę bez efektu — został usunięty).

### A4. Blokada/odblokowanie użytkownika i dziennik działań (nowość, najważniejszy test tej części)
Na `/Admin/Users` zablokuj dowolnego użytkownika **innego niż `a1`** (np. `u2`) na 1 dzień, potem
od razu go odblokuj. Następnie wejdź na `/Admin/AuditLog`. **Oczekiwane:** widoczne dwa nowe
wpisy (blokada i odblokowanie) z nazwą administratora (`a1`), nazwą celu (`u2`) i znacznikiem
czasu — **nawet jeśli** wcześniej próbowałbyś to zweryfikować inaczej, samo pojawienie się wpisu
zaraz po akcji jest tym, co testujemy (wpis ma być zapisywany w tej samej transakcji co akcja).

**STOP.** Wyloguj się z konta `a1`. Zanim zalogujesz się jako zwykły użytkownik — zapytaj
użytkownika, czy kontynuować.

---

## Część 2 — Zwykły użytkownik

### U1. Rejestracja bez akceptacji regulaminu — musi się nie udać (kluczowy test)
Wejdź na stronę rejestracji. Wypełnij wszystkie pola poprawnie, ale **nie zaznaczaj** checkboxa
akceptacji regulaminu ani oświadczenia o wieku. Kliknij „Zarejestruj". **Oczekiwane:** rejestracja
się nie powiedzie, formularz pokazuje błąd walidacji przy obu checkboxach — konto **nie może**
powstać. To naprawiona luka: wcześniej dało się to ominąć.

Następnie wypełnij formularz ponownie, tym razem **zaznaczając oba checkboxy**, z nowym adresem
e-mail. **Oczekiwane:** rejestracja się udaje. Po drodze sprawdź: link „regulamin" w checkboxie
otwiera treść regulaminu (nowa karta), a nad przyciskiem „Zarejestruj" jest krótka informacja
o administratorze z linkami do „Polityki prywatności w skrócie" i „Pełnej polityki prywatności".

### U2. Logowanie i podstawowe przeglądanie (regresja)
Zaloguj się na nowo założone konto (albo `u1`, hasło `TestPass123!`). **Oczekiwane:** logowanie
działa, strona główna i `/Browse` pokazują ogłoszenia.

### U3. Ostrzeżenie o danych osobowych w opisie ogłoszenia (nowość)
Dodaj nowe ogłoszenie, w opisie wpisując coś w rodzaju „kontakt: test@przyklad.pl". **Oczekiwane:**
pojawia się ostrzeżenie, że opis wygląda na zawierający dane kontaktowe, i formularz wymaga
świadomego potwierdzenia przed wysłaniem (nie blokuje publikacji całkowicie). Po dodaniu wróć do
panelu admina (`/Admin/Items`, jeśli masz jeszcze dostęp — jeśli nie, pomiń tę część) i sprawdź,
czy to ogłoszenie jest teraz oznaczone jako „do przejrzenia".

### U4. Widoczność szkoły — tylko za zgodą i tylko dla zalogowanych (kluczowy test)
W Ustawieniach profilu włącz przełącznik „Pokaż moją szkołę przy moich ogłoszeniach" (domyślnie
wyłączony). Otwórz własne ogłoszenie (albo stronę profilu) w **zwykłej karcie** — nazwa szkoły
powinna być widoczna. Potem otwórz **tę samą stronę w oknie prywatnym/incognito** (bez logowania)
— nazwa szkoły **nie powinna** być widoczna wcale, niezależnie od przełącznika.

### U5. Dane kontaktowe sprzedającego — tylko przy aktywnym ogłoszeniu (nowość)
Otwórz cudze ogłoszenie (od użytkownika z aktywnym, widocznym ogłoszeniem) i kliknij przycisk
kontaktu — dane kontaktowe powinny się pokazać. Jeśli uda się znaleźć profil użytkownika **bez
żadnych aktywnych ogłoszeń**, sprawdź, że zamiast danych kontaktowych widać komunikat, że ten
użytkownik nie ma obecnie aktywnych ogłoszeń, więc dane nie są udostępniane.

### U6. Pobranie danych osobowych (regresja kluczowej funkcji RODO)
W Ustawieniach → Dane osobowe → „Pobierz". **Oczekiwane:** pobiera się plik JSON z sekcjami
`konto`, `ogloszenia`, `ulubione` (otwórz plik i sprawdź pobieżnie, że sekcje nie są puste, jeśli
masz na koncie jakieś ogłoszenie/ulubione z wcześniejszych kroków).

### U7. Stopka — jeden adres kontaktowy i komplet linków (regresja)
Na dowolnej stronie sprawdź stopkę: linki „Regulamin", „Polityka prywatności", „Polityka
prywatności w skrócie" oraz „Kontakt" prowadzący do `support@textbooker.pl`. **Oczekiwane:**
wszystkie cztery linki działają i prowadzą do właściwych stron, adres kontaktowy jest wszędzie
taki sam.

---

## Czego celowo NIE robimy

- **Nie testuj blokady logowania przez 10 nieudanych prób.** Jedna próba z błędnym hasłem
  wystarczy, żeby zobaczyć, że komunikat błędu jest ogólny (nie zdradza, czy konto istnieje).
  Doprowadzenie do faktycznej blokady zablokuje to konto na 2 godziny i utrudni dalsze testy.
- **Nie próbuj wyczerpać limitu ujawnień danych kontaktowych** (60/godzinę) — to wymagałoby
  dziesiątek kliknięć bez realnej wartości testowej na tym etapie.
- **Nie usuwaj żadnego konta testowego** (`/Identity/Account/Manage/DeletePersonalData`) bez
  wyraźnej zgody użytkownika — to nieodwracalne.
