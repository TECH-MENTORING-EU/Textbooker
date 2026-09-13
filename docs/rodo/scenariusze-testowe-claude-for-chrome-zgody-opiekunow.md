# TextBooker - scenariusze dla Claude for Chrome: zgody opiekunów i rejestracja

Uzupełnienie dokumentu `scenariusze-testowe-claude-for-chrome.md`. Ten plik pokrywa zmiany dotyczące rejestracji ucznia niepełnoletniego, zgody opiekuna, wygasania oczekujących zgód oraz panelu administracyjnego.

## Zmiany w przebiegu od poprzedniej wersji dokumentu

Poniższe scenariusze zostały uzupełnione po poprawkach z code review PR #117. Najważniejsze zmiany w rzeczywistym działaniu aplikacji:

- **Potwierdzenie zgody opiekuna wymaga teraz dwóch kroków.** Samo otwarcie linku (GET) tylko pokazuje stronę z podsumowaniem i przyciskiem `Potwierdzam zgodę` - nie aktywuje niczego. Zgoda zostaje zapisana dopiero po kliknięciu przycisku (POST). Ma to chronić przed przypadkową aktywacją, gdy link otworzy skaner poczty albo podgląd linku, zanim zrobi to opiekun.
- **Uczeń niepełnoletni dostaje teraz własny e-mail z potwierdzeniem adresu**, niezależnie od e-maila do opiekuna z prośbą o zgodę. Aktywacja konta (`IsVisible = true`) następuje dopiero, gdy **oba** warunki są spełnione: uczeń potwierdził własny e-mail ORAZ opiekun potwierdził zgodę - w dowolnej kolejności.
- **Strona "Wyślij ponownie e-mail z linkiem aktywacyjnym" pokazuje teraz zawsze ten sam, ogólny komunikat**, niezależnie od tego, czy podany adres istnieje, czy jest kontem niepełnoletnim oczekującym na zgodę opiekuna, czy adresem nieznanym. Ma to uniemożliwić odgadnięcie, czy dany e-mail jest zarejestrowany (ochrona przed enumeracją kont).
- **Ponowne wysłanie linku do opiekuna nie przedłuża już oryginalnego terminu wygaśnięcia konta.** Jeśli pierwotny termin już minął, ponowne wysłanie po cichu nic nie wysyła (bez ujawniania tego faktu proszącemu).
- Pole e-maila opiekuna w formularzu rejestracji zachowuje widoczność i pokazuje błąd walidacji także po pełnym przeładowaniu strony (np. gdy POST się nie powiedzie z innego powodu), a nie tylko przy wymianie fragmentu przez htmx.
- Komunikaty, które wcześniej były po angielsku (np. `Invalid confirmation link.`, temat maila `Confirm your email`, komunikaty walidacji roku urodzenia i e-maila opiekuna), zostały przetłumaczone na polski.

## Zasady bezpieczeństwa

1. Nie wysyłaj formularza rejestracji i nie twórz nowego konta bez wyraźnego polecenia człowieka.
2. Jeśli potrzebujesz zalogować konto, otworzyć skrzynkę e-mail albo wykonać test wymagający danych dostępowych, poproś człowieka o odpowiedni adres e-mail, login i hasło. Nie zgaduj danych i nie korzystaj z haseł zapisanych w tym dokumencie ani w innych instrukcjach.
3. Nie wpisuj haseł ani adresów e-mail bezpośrednio w przeglądarce bez wcześniejszego przekazania ich przez człowieka w bieżącej rozmowie. Nie wyświetlaj haseł w raporcie.
4. Każda wiadomość zawierająca prośbę do człowieka musi mieć polecenie w ostatniej linii. Ostatnia linia musi zaczynać się od `polecenie:` i zawierać wszystkie potrzebne czynności. Nie dopisuj nic po tej linii.
5. Nie używaj prawdziwych adresów e-mail ani danych prawdziwych osób. Do testów używaj danych testowych podanych przez człowieka albo danych jednoznacznie testowych.
6. Nie zmieniaj i nie usuwaj istniejących kont, zgód, szkół ani ogłoszeń.
7. Jeżeli scenariusz wymaga dostępu do bazy danych albo upływu czasu, oznacz go jako `NIE SPRAWDZONO`, chyba że środowisko testowe udostępnia bezpieczny, przygotowany przypadek.
8. Dla każdego testu zapisz adres URL, dane użyte w formularzu, wynik (`OK`, `BŁĄD` albo `NIE SPRAWDZONO`) oraz widoczny komunikat.

## Warunki wstępne

- Ustal działający adres aplikacji zgodnie z głównym dokumentem testowym.
- Do testów formularza użyj strony `/Identity/Account/Register`.
- Do testów administratora użyj konta administratora wskazanego w głównym dokumencie.
- Do testów panelu zgód otwórz `/Admin/GuardianConsents`.
- Nie zakładaj, że data wygaśnięcia tokenu oznacza wygaśnięcie potwierdzonej zgody.
- Jeśli dane logowania nie są dostępne, zatrzymaj się przed logowaniem i poproś o nie zgodnie z zasadą ostatniej linii `polecenie:`.

## R1. Rejestracja - walidacja roku urodzenia

Otwórz formularz rejestracji i sprawdź następujące przypadki bez wysyłania formularza:

1. Pozostaw rok urodzenia pusty.
2. Wpisz rok przyszły.
3. Wpisz rok wcześniejszy niż 1910.
4. Wpisz prawidłowy rok osoby mającej co najmniej 16 lat.

Oczekiwane:

- formularz pozwala przejść przez poprawne dane bez błędów walidacji;
- rok przyszły i zbyt stary są odrzucane;
- komunikaty walidacyjne są po polsku;
- zmiana roku nie powoduje błędu 500 ani utraty pozostałych danych formularza.

## R2. Pole e-maila opiekuna dla ucznia niepełnoletniego

W formularzu rejestracji wpisz rok oznaczający wiek poniżej 16 lat. Sprawdź pole e-maila opiekuna.
Następnie zmień rok na taki, który oznacza co najmniej 16 lat, i sprawdź pole ponownie.

Oczekiwane dla wieku poniżej 16 lat:

- pole e-maila opiekuna pojawia się po zmianie roku;
- pole jest wymagane;
- zmiana roku nie przeładowuje całej strony;
- wpisana wartość e-maila opiekuna zostaje zachowana po wymianie fragmentu formularza.

Oczekiwane dla wieku co najmniej 16 lat:

- pole e-maila opiekuna znika;
- nie jest wymagane przy wysłaniu formularza;
- pozostałe pola formularza pozostają bez zmian.

Jeśli interfejs pokazuje błąd albo pole nie reaguje, zapisz URL i kroki odtworzenia.

Dodatkowo sprawdź zachowanie po pełnym przeładowaniu strony (nie tylko po wymianie fragmentu przez htmx): wpisz rok oznaczający wiek poniżej 16 lat, wpisz niepoprawny e-mail opiekuna i wywołaj taki błąd walidacji serwerowej, który powoduje pełne przeładowanie strony (np. brak wybranej szkoły). Oczekiwane:

- pole e-maila opiekuna pozostaje widoczne po przeładowaniu strony (nie chowa się z powrotem);
- błąd walidacji dla e-maila opiekuna jest pokazany przy tym polu, po polsku.

## R3. Automatyczne przypisanie szkoły po adresie e-mail

W polu e-mail wpisz adres z domeną szkoły skonfigurowaną w środowisku testowym. Przejdź do następnego pola albo wywołaj zmianę wartości.

Oczekiwane:

- lista szkoły zostaje uzupełniona przez żądanie w tle;
- wybrana szkoła odpowiada domenie e-maila;
- formularz nie przeładowuje całej strony;
- dla nieznanej domeny można wybrać szkołę ręcznie;
- komunikaty i etykiety są po polsku.

## R4. Walidacja e-maila opiekuna

Przy roku oznaczającym wiek poniżej 16 lat sprawdź kolejno:

1. pusty e-mail opiekuna;
2. niepoprawny format e-maila opiekuna;
3. ten sam adres e-mail ucznia i opiekuna;
4. dwa różne, poprawne adresy testowe.

Nie wysyłaj poprawnego formularza, jeśli utworzyłoby to konto bez zgody człowieka.

Oczekiwane:

- pierwsze trzy przypadki są odrzucane;
- błędy są pokazane przy właściwym polu i są po polsku;
- poprawne, różne adresy przechodzą walidację formularza;
- walidacja serwerowa nie może być możliwa do obejścia przez usunięcie atrybutu `required` w przeglądarce.

## R5. Oczekująca zgoda - ścieżka bez potwierdzenia

Wykonuj tylko wtedy, gdy człowiek udostępni przygotowane konto testowe ucznia niepełnoletniego albo wyraźnie zezwoli na utworzenie konta testowego.

Po rejestracji ucznia niepełnoletniego uczeń dostaje **dwa** osobne e-maile: własne potwierdzenie adresu e-mail (temat `Potwierdź swój adres e-mail`) oraz e-mail do opiekuna z prośbą o zgodę. Sprawdź oba, a także stronę potwierdzenia rejestracji i ponownego wysłania linku.

Oczekiwane:

- uczeń otrzymuje własny link potwierdzający e-mail, osobny od linku dla opiekuna;
- treść e-maila do ucznia jest po polsku i wspomina, że konto wymaga też zgody opiekuna, zanim zostanie aktywowane;
- strona potwierdzenia rejestracji informuje, że konto ucznia będzie aktywne po zgodzie opiekuna;
- informacja o ważności linku jest po polsku;
- tekst używa określenia `uczeń` lub `ucznia`, a nie `dziecko`;
- konto pozostaje nieaktywne (niewidoczne), dopóki NIE są spełnione oba warunki: uczeń potwierdził własny e-mail i opiekun potwierdził zgodę.

Na stronie ponownego wysłania linku (`ResendEmailConfirmation`) sprawdź, że komunikat po wysłaniu formularza jest **taki sam, ogólny**, niezależnie od tego, czy konto istnieje - patrz R11 (ochrona przed enumeracją kont). Nie oczekuj już osobnego komunikatu w stylu "link trafił do opiekuna".

Jeśli nie ma bezpiecznego konta testowego albo dostępu do skrzynki opiekuna, oznacz test jako `NIE SPRAWDZONO`.

## R6. Potwierdzenie zgody opiekuna

Wykonuj tylko z przygotowanym, jednorazowym linkiem testowym. Potwierdzenie odbywa się teraz w dwóch krokach: otwarcie linku (GET) tylko pokazuje podsumowanie, samo potwierdzenie (POST) następuje po kliknięciu przycisku.

### Krok 1: otwarcie linku (GET) nie może nic zmieniać

Otwórz poprawny, jeszcze niewykorzystany link w przeglądarce, ale **nie klikaj** przycisku potwierdzenia.

Oczekiwane:

- strona pokazuje podsumowanie po polsku i przycisk `Potwierdzam zgodę` w formularzu;
- samo załadowanie strony (GET) NIE potwierdza zgody i NIE aktywuje konta - to sprawdza się np. otwierając ten sam link drugi raz: podsumowanie powinno wyglądać tak samo, a nie jak "link już wykorzystany";
- jeśli masz dostęp do panelu administratora zgód (`/Admin/GuardianConsents`), sprawdź, że status zgody nadal jest `Oczekuje` po samym GET, bez klikania przycisku.

### Krok 2: kliknięcie `Potwierdzam zgodę` (POST)

Na stronie z kroku 1 kliknij przycisk `Potwierdzam zgodę`.

Oczekiwane:

- pojawia się komunikat po polsku;
- komunikat informuje, że zgoda opiekuna została potwierdzona;
- jeśli uczeń już wcześniej potwierdził własny e-mail, komunikat mówi, że konto jest aktywne; jeśli nie, komunikat mówi, że konto zostanie aktywowane po potwierdzeniu e-maila przez ucznia (patrz R12);
- po odświeżeniu strony nie następuje ponowne wykonanie potwierdzenia (odświeżenie to zwykłe GET, formularz POST nie wysyła się ponownie).

Następnie użyj tego samego linku ponownie (GET, a potem ponowna próba POST jeśli to możliwe).

Oczekiwane:

- drugie użycie jest odrzucone;
- komunikat jest po polsku i informuje, że link został już wykorzystany;
- nie powstaje drugie potwierdzenie ani błąd 500.

## R7. Wygasająca zgoda oczekująca

Ten scenariusz wymaga przygotowanego w bazie testowego rekordu niepotwierdzonej zgody z przeterminowanym `ExpiresAtUtc`. Claude for Chrome nie powinien sam zmieniać danych w bazie ani czekać siedmiu dni.

Jeśli taki rekord jest dostępny, otwórz jego link potwierdzający.

Oczekiwane:

- link jest odrzucony;
- komunikat o wygaśnięciu jest po polsku;
- użytkownik jest informowany, że może poprosić o nowy link;
- nie ma błędu 500.

Po wykonaniu cleanupu przez aplikację, w bezpiecznym środowisku testowym sprawdź poza przeglądarką, że wygasła niepotwierdzona zgoda usuwa konto ucznia oraz powiązaną zgodę. Jeśli nie ma przygotowanego rekordu i dostępu do weryfikacji, oznacz test jako `NIE SPRAWDZONO`.

Dodatkowo: ponowne wysłanie linku do opiekuna (strona `ResendEmailConfirmation`) **nie przedłuża** już oryginalnego terminu wygaśnięcia konta (`ExpiresAtUtc` ustalanego przy rejestracji). Jeśli jest dostępny testowy rekord z terminem, który już minął, wywołaj dla niego ponowne wysłanie i sprawdź w bezpieczny sposób (panel administratora albo baza testowa), że nowy e-mail do opiekuna NIE został wysłany, a termin wygaśnięcia się nie zmienił. Odpowiedź na stronie dla proszącego powinna mimo to pokazać ten sam, ogólny komunikat co zawsze (patrz R11) - bez ujawniania, że coś poszło nie tak. Bez przygotowanego rekordu oznacz jako `NIE SPRAWDZONO`.

## R8. Potwierdzona zgoda nie wygasa

Ten scenariusz wymaga przygotowanego konta z potwierdzoną zgodą oraz datą `ExpiresAtUtc` w przeszłości albo wykonania cleanupu po upływie terminu.

Oczekiwane:

- potwierdzone konto nadal istnieje i jest aktywne;
- potwierdzona zgoda nie jest usuwana przez cleanup;
- panel administratora nie pokazuje dla niej daty wygaśnięcia, tylko `Nie wygasa`;
- historyczna data wygaśnięcia tokenu nie jest traktowana jako wygaśnięcie zgody.

Nie zmieniaj ręcznie dat w produkcyjnej ani współdzielonej bazie. Bez przygotowanego fixture'u oznacz test jako `NIE SPRAWDZONO`.

## R9. Panel administratora zgód

Zaloguj się jako administrator i otwórz `/Admin/GuardianConsents`.

Sprawdź:

- tabelę zgód oczekujących i potwierdzonych;
- filtrowanie po użytkowniku, e-mailu opiekuna i statusie;
- statusy `Oczekuje`, `Potwierdzona` i `Anonimizowana`;
- datę wygaśnięcia dla zgody oczekującej;
- napis `Nie wygasa` dla zgody potwierdzonej;
- paginację przy większej liczbie rekordów;
- brak błędów 500 i komunikatów po angielsku.

Nie edytuj, nie usuwaj i nie anonimizuj rekordów ręcznie.

## R10. Język komunikatów zgody

Przejdź przez dostępne bezpieczne ścieżki błędne: pusty link, niepoprawny link, wykorzystany link oraz wygasły link testowy.

Oczekiwane:

- wszystkie komunikaty widoczne dla użytkownika są po polsku;
- określenia dotyczą ucznia i opiekuna;
- nie pojawiają się angielskie teksty typu `Invalid confirmation link`, `Your account is now active`, `This confirmation link has expired`, `Confirm your email`, `Please confirm your account`, `Year of birth is required` ani `Guardian email must be a valid email address`;
- logi techniczne (widoczne tylko w konsoli/logu serwera, nie na stronie) nie są traktowane jako komunikaty UI - nie muszą być po polsku.

## R11. Ochrona przed enumeracją kont na stronie ponownego wysłania

Wykonuj tylko z danymi testowymi. Otwórz `/Identity/Account/ResendEmailConfirmation` i po kolei wpisz:

1. adres e-mail nieistniejącego konta;
2. adres e-mail istniejącego, już potwierdzonego konta dorosłego;
3. adres e-mail istniejącego konta niepełnoletniego, które wciąż czeka na zgodę opiekuna (tylko jeśli dostępne jest przygotowane konto testowe - w przeciwnym razie pomiń ten punkt i oznacz go `NIE SPRAWDZONO`).

Oczekiwane:

- we wszystkich trzech przypadkach strona pokazuje **dokładnie ten sam** komunikat ogólny (np. "Jeśli podany adres e-mail jest powiązany z kontem oczekującym na potwierdzenie, wysłaliśmy odpowiednią wiadomość z instrukcjami.");
- treść ani forma komunikatu nie zdradza, czy konto istnieje, jest potwierdzone, czy jest kontem niepełnoletnim oczekującym na opiekuna;
- brak błędu 500 w żadnym z przypadków.

## R12. Niezależna kolejność: potwierdzenie e-maila ucznia i zgody opiekuna

Wykonuj tylko z przygotowanym kontem testowym ucznia niepełnoletniego, dla którego dostępne są oba linki (własny link potwierdzający e-mail ucznia oraz link do potwierdzenia zgody opiekuna).

Sprawdź obie kolejności osobno (najlepiej na dwóch różnych kontach testowych, żeby nie mieszać stanu):

**Kolejność A - najpierw uczeń, potem opiekun:**

1. Otwórz link potwierdzający e-mail ucznia i potwierdź.
2. Sprawdź komunikat - powinien informować, że e-mail jest potwierdzony, ale konto czeka jeszcze na zgodę opiekuna.
3. Otwórz link zgody opiekuna i kliknij `Potwierdzam zgodę` (patrz R6, krok 2).
4. Sprawdź komunikat po potwierdzeniu zgody - powinien informować, że konto jest teraz aktywne.

**Kolejność B - najpierw opiekun, potem uczeń:**

1. Otwórz link zgody opiekuna i kliknij `Potwierdzam zgodę`.
2. Sprawdź komunikat - powinien informować, że zgoda jest potwierdzona, ale konto zostanie aktywowane po potwierdzeniu e-maila przez ucznia.
3. Otwórz link potwierdzający e-mail ucznia i potwierdź.
4. Sprawdź komunikat po potwierdzeniu e-maila - powinien informować, że konto jest teraz aktywne.

Oczekiwane w obu kolejnościach:

- konto NIE jest aktywne (niewidoczne) po wykonaniu tylko jednego z dwóch kroków;
- konto staje się aktywne dopiero po wykonaniu obu kroków, niezależnie od kolejności;
- wszystkie komunikaty pośrednie i końcowe są po polsku.

## Raport

```text
Adres testowy: ...

| ID | Wynik | URL / dowód / uwagi |
|----|-------|----------------------|
| R1 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R2 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R3 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R4 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R5 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R6 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R7 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R8 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R9 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R10 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R11 | OK / BŁĄD / NIE SPRAWDZONO | ... |
| R12 | OK / BŁĄD / NIE SPRAWDZONO | ... |
```

## Kryteria błędu blokującego

Za błąd blokujący uznaj:

- możliwość aktywacji konta ucznia bez potwierdzenia zgody;
- aktywację zgody (albo konta) samym otwarciem linku (GET), bez kliknięcia przycisku potwierdzenia;
- aktywację konta, gdy tylko jeden z dwóch warunków (e-mail ucznia, zgoda opiekuna) jest spełniony;
- możliwość użycia tego samego linku więcej niż raz;
- akceptowanie wygasłego linku;
- usunięcie aktywnego konta po potwierdzeniu zgody;
- brak usunięcia niepotwierdzonego konta po wygaśnięciu zgody;
- utratę danych formularza podczas zmiany roku lub szkoły;
- różne komunikaty na stronie ponownego wysłania linku dla istniejącego i nieistniejącego adresu e-mail (enumeracja kont);
- przedłużenie terminu wygaśnięcia konta przez samo ponowne wysłanie linku do opiekuna;
- angielski komunikat w miejscu widocznym dla użytkownika;
- błąd 500 w dowolnej ścieżce zgody lub rejestracji.
