# TextBooker - plan testow przed wdrozeniem

Instrukcja dla Claude for Chrome do samodzielnego wykonania testu smoke i regresji przed wdrozeniem. Zakres obejmuje najwazniejsze funkcje widoczne dla uzytkownika oraz krytyczne funkcje administracyjne.

## Zasady pracy

1. Wykonuj testy samodzielnie: ustal adres aplikacji, klikaj, wpisuj dane, interpretuj wynik i zbieraj dowody.
2. Nie loguj ani nie wylogowuj. Czlowiek podejdzie do komputera trzy razy: aby zalogowac `u1`, aby przelogowac na `a1` oraz aby wylogowac przed faza anonimowa. W fazie anonimowej przygotuj poprawny formularz rejestracji, ale zatrzymaj sie przed koncowym kliknieciem `Zarejestruj`.
3. Nie przerywaj po jednym bledzie. Zapisz blad, jego objawy i kontynuuj, chyba ze aplikacja nie laduje sie wcale.
4. Zaczynaj od konta zwyklego uzytkownika, potem testuj administratora, a na koncu niezalogowanego.
5. Nie usuwaj kont, ogloszen, szkol ani administratorow. Nie zmieniaj hasel ani adresow e-mail kont testowych.
6. Po testach wymagajacych zmiany danych przywroc stan opisany w danym scenariuszu.
7. Wynik testu oznacz jako `OK`, `BLAD` albo `NIE SPRAWDZONO`. Dla bledu zapisz URL, widoczny komunikat i kroki odtworzenia.
8. Adres aplikacji ustal kolejno z otwartych kart, a potem pod adresami `https://localhost:5001`, `http://localhost:5000` i `https://localhost:7001`. Pierwszy dzialajacy adres zapisz w raporcie.

## Dane testowe

- Uzytkownik: `u1` / `TestPass123!`
- Administrator: `a1` / `TestPass123!`
- Konto do blokowania przez administratora: `u2`
- Szkola kont `u1` i `a1`: Hogwort
- Konta z innych szkol maja prefiksy `r2_` i `r3_`
- Nowe konto do rejestracji: `t.osmanowki@outlook.com` / `TestPass123!`

## Faza 1 - zwykly uzytkownik (`u1`)

### Przerwa na przelogowanie nr 1 - zawsze przed startem

Zanim wykonasz jakikolwiek test, napisz i zaczekaj. Nie sprawdzaj, kto jest obecnie zalogowany, i nie wykonuj zadnej akcji wymagajacej konta:

> Testy rozpoczynam od zwyklego uzytkownika. Adres aplikacji: `<adres>`.
>
> polecenie: zaloguj sie jako `u1` / `TestPass123!` i napisz `ok`.

### U1. Nawigacja i strona glowna

Sprawdz menu konta, przycisk dodania ogloszenia, sekcje strony glownej, popularne przedmioty, ostatnio dodane ogloszenia i stopke. Nie powinno byc bledow 500, pustych kafelkow ani niedzialajacych obrazkow. Konto zwykle nie moze widziec panelu administratora.

### U2. Przegladanie, wyszukiwanie i filtry

Na `/Browse` sprawdz, czy kafelki maja zdjecie, tytul, cene i tagi. Przewin do konca, aby potwierdzic automatyczne doladowanie. Przetestuj po kolei filtr przedmiotu, klasy, poziomu, zakresu cenowego oraz wyszukiwanie fragmentu tytulu. Przetestuj takze przypadek `cena min > cena max` i fraze bez wynikow. Na koncu wyczysc filtry.

Regresja (wyscig czyszczenia filtrow): ustaw tylko `Cena min.` na dowolna wartosc, poczekaj na zaladowanie wynikow i pozostan z kursorem w polu ceny. Nie wychodzac z pola, kliknij `Wyczyść filtry`. Oczekiwane: lista przeładowuje sie na pelna (bez filtrow), adres URL konczy sie czystym `/Browse`, a pola formularza sa puste. Powtorz probe z `Cena max.`.

### U3. Izolacja szkol i karta ogloszenia

Otworz co najmniej piec ogloszen z `/Browse`. Sprzedajacy powinni nalezec do Hogwortu: `u1`, `u4`, `a1` albo kont `r1_`; nie powinno byc `r2_` ani `r3_`. Zapisz URL jednego cudzego ogloszenia. Na jego karcie potwierdz komplet danych, dzialanie galerii jezeli sa minimum dwa zdjecia, link do profilu sprzedajacego oraz brak licznika wyswietlen dla osoby niebedacej wlascicielem.

### U4. Ulubione i profil sprzedajacego

Dodaj zapisane cudze ogloszenie do ulubionych, sprawdz natychmiastowa zmiane stanu przycisku i obecnosc na `/Profile/Favorites`, usun je, odswiez liste, a nastepnie dodaj ponownie. Wejdz na profil sprzedajacego: ogloszenia maja byc widoczne. Zakladka `Ulubione` ma byc widoczna tylko, gdy ulubione sprzedajacego sa publiczne; w przeciwnym razie nie moze byc martwego linku.

### U5. Dodanie ogloszenia z ostrzezeniem danych kontaktowych

Dodaj ogloszenie z co najmniej jednym obrazem `.jpg`, `.jpeg` lub `.png`, tytulem z listy, cena dodatnia i opisem zawierajacym `test@przyklad.pl`. W podsumowaniu potwierdz publikacje. Oczekiwane: formularz pokazuje potwierdzenie swiadomego umieszczenia danych kontaktowych przed wyslaniem, wybrane zdjecia pozostaja na formularzu, a po zaznaczeniu potwierdzenia ogloszenie zostaje utworzone. Zapisz tytul i URL ogloszenia do kolejnych testow.

### U6. Walidacja dodawania i edycja wlasnego ogloszenia

Na `/Add` sprawdz pusty formularz, cene `0`, zbyt dlugi stan oraz plik inny niz obraz. Komunikaty musza byc po polsku. Otworz ogloszenie z `U5`, zmien cene i opis bez dodawania zdjec, a potem zapisz. Oczekiwane: brak bledu o wymaganych zdjeciach, dotychczasowe zdjecia zostaja zachowane, a karta pokazuje zmienione dane i date edycji.

### U7. Rezerwacja i wyswietlenia

Na wlasnym ogloszeniu wlacz rezerwacje, odswiez strone i potwierdz komunikat oraz oznaczenie kafelka. Nastepnie wylacz rezerwacje i potwierdz, ze komunikat znika. Sprawdz, ze wlasciciel widzi licznik wyswietlen na wlasnym ogloszeniu.

### U8. Ustawienia konta i prywatnosc

Na stronie ustawien sprawdz:

- wlaczenie widocznosci szkoly przy ogloszeniach i jej widocznosc dla zalogowanego;
- odrzucenie zapisu bez zadnej formy kontaktu;
- odrzucenie WhatsApp bez numeru telefonu oraz Messengera bez nazwy;
- brak mozliwosci zmiany przypisanej szkoly;
- zmiane hasla z blednym obecnym haslem bez zmiany hasla;
- pobranie danych osobowych: JSON zawiera konto, ogloszenia i ulubione.

Regresja (kłamiacy formularz po nieudanym zapisie): po kazdym odrzuconym zapisie (np. WhatsApp bez numeru telefonu albo wszystkie kanaly kontaktu wylaczone) zweryfikuj na renderowanej stronie, ze:

- nie pojawia sie komunikat `Twój profil został zaktualizowany.`;
- komunikaty bledow pasuja do stanu przełacznikow widocznego na formularzu (np. gdy formularz pokazuje właczony e-mail, nie moze byc bledu o braku formy kontaktu, chyba ze wlasnie go wylaczono w tej probie zapisu).

Po sprawdzeniu odswiez strone przyciskiem przegladarki i potwierdz, ze formularz pokazuje zapisane (spojne z baza) ustawienia.

Na koncu przywroc dzialajaca forme kontaktu (e-mail) i zostaw wlaczona widocznosc szkoly, aby sprawdzic ja anonimowo w fazie 3.

### U9. Kontakt i strony bledow

Na cudzym aktywnym ogloszeniu kliknij `Zapytaj o przedmiot`. Dane kontaktowe maja pokazac sie zalogowanemu. Na wlasnym ogloszeniu przycisku kontaktu nie moze byc. Otworz `/Book/999999`; oczekiwany jest przyjazny blad 404, bez bledu serwera.

## Faza 2 - administrator (`a1`)

### Przerwa na przelogowanie nr 2 - po fazie uzytkownika

> Faza uzytkownika zostala zakonczona. Teraz potrzebuje konta administratora.
>
> polecenie: zaloguj sie jako `a1` / `TestPass123!` i napisz `ok`.

### A1. Panel i podsumowanie

Otworz `/Admin`. Sprawdz nawigacje: Podsumowanie, Uzytkownicy, Administratorzy, Szkoly, Ogloszenia i Dziennik dzialan. Podsumowanie musi zawierac liczby uzytkownikow, ogloszen i szkol.

### A2. Ogloszenia i moderacja

Na `/Admin/Items` sprawdz wyszukiwanie lub filtrowanie, otworz kilka kart ogloszen oraz znajdz ogloszenie z `U5`. Musi byc oznaczone jako `Do przejrzenia`, jezeli opis zawiera dane kontaktowe. Zwykle ogloszenia nie powinny miec tego oznaczenia.

### A3. Uzytkownicy, blokada i dziennik

Na `/Admin/Users` sprawdz wyszukiwanie `u1` i brak niedzialajacego przycisku akcji. Zablokuj `u2` na jeden dzien, potwierdz zmiane statusu, a potem natychmiast odblokuj. Na `/Admin/AuditLog` sprawdz oba wpisy: administrator, typ akcji, cel, czas i parametr liczby dni. `u2` musi pozostac odblokowany po tescie.

### A4. Szkoly i administratorzy

Na `/Admin/Schools` sprawdz liste szkol, domeny, liczbe uzytkownikow i filtr nieaktywnych szkol. Na `/Admin/Admins` potwierdz wyswietlanie listy, ale nie zmieniaj rol. Nie dodawaj, nie edytuj i nie dezaktywuj szkol.

### A5. Izolacja szkol dla administratora

Na liscie administracyjnej znajdz ogloszenie sprzedawcy `r2_` albo `r3_` i otworz jego publiczny URL. Zalogowany `a1` z Hogwortu powinien dostac 404, mimo ze widzi rekord w panelu. Zapisz URL do testu anonimowego.

## Faza 3 - niezalogowany

### Przerwa na przelogowanie nr 3 - po fazie administratora

> Fazy uzytkownika i administratora zostaly zakonczone. Zostaly testy publicznej wersji aplikacji.
>
> polecenie: wyloguj sie i napisz `ok`.

### W1. Dostep publiczny i stopka

Sprawdz strone glowna i `/Browse`: powinny ladowac sie bez logowania, miec dzialajace wyszukiwanie oraz filtry, a naglowek powinien oferowac logowanie i rejestracje. Sprawdz stopke: linki do mapy strony, regulaminu, polityk prywatnosci i kontaktu maja dzialac, a adres kontaktowy ma byc jednolity.

### W2. Prywatnosc ogloszen i szkol

Otworz URL cudzego ogloszenia zapisany w `U3`: nazwa szkoly nie moze byc widoczna. Otworz URL wlasnego ogloszenia z `U5`, mimo wlaczonej widocznosci szkoly: szkola rowniez nie moze byc widoczna. Kliknij `Zapytaj o przedmiot`; aplikacja ma przekierowac do logowania, bez ujawnienia kontaktu.

### W3. Dostep do ogloszen innych szkol

Otworz URL ogloszenia z innej szkoly zapisany w `A5`. Dla anonimowego uzytkownika karta ma byc dostepna. Na `/Browse` sprawdz przynajmniej jedno ogloszenie `r2_` lub `r3_`.

### W4. Strony chronione i rejestracja

Otworz `/Add`, `/Profile`, `/Identity/Account/Manage` i `/Admin`. Zadne z tych miejsc nie moze udostepnic chronionych danych lub formularzy niezalogowanemu. Na stronie rejestracji sprawdz link do regulaminu, informacje o przetwarzaniu danych oraz automatyczne dopasowanie szkoly po adresie z domeny `hogwart.edu.pl`.

Nastepnie przygotuj poprawny formularz rejestracji: wpisz e-mail `t.osmanowki@outlook.com`, haslo `TestPass123!` w obu polach, zaznacz akceptacje regulaminu i oswiadczenie o wieku. Jesli szkola nie zostanie uzupelniona automatycznie, wybierz ja recznie. Potwierdz, ze formularz nie ma bledow walidacji, ale sam nie klikaj `Zarejestruj`.

Napisz i zaczekaj:

> Formularz rejestracji jest wypelniony poprawnie i gotowy do wyslania. Nie klikam przycisku, poniewaz utworzy prawdziwe konto.
>
> polecenie: kliknij `Zarejestruj` i napisz `ok`.

Po potwierdzeniu sprawdz, ze konto zostalo utworzone i zalogowane, a aplikacja pokazuje komunikat sukcesu lub prawidlowo przekierowuje. Zapisz URL koncowy i wynik rejestracji w raporcie.

## Raport koncowy

Oddaj jeden raport w tej strukturze:

```text
Adres testowy: ...
Wersja/commit: ...

## Uzytkownik
| ID | Wynik | Dowod / uwagi |
| U1-U9 | OK / BLAD / NIE SPRAWDZONO | ... |

## Administrator
| ID | Wynik | Dowod / uwagi |
| A1-A5 | OK / BLAD / NIE SPRAWDZONO | ... |

## Niezalogowany
| ID | Wynik | Dowod / uwagi |
| W1-W4 | OK / BLAD / NIE SPRAWDZONO | ... |

## Bledy blokujace wdrozenie
- ...

## Ryzyka i uwagi
- ...

## Pominiete testy
- ID scenariusza, powod pominiecia i wplyw na pewnosc przed wdrozeniem: ...

## Propozycje kolejnych scenariuszy
- Scenariusz, ryzyko ktore pokrywa i uzasadnienie: ...

## Stan po testach
- Czy u2 zostal odblokowany: ...
- Tytul i URL dodanego ogloszenia: ...
- Czy rezerwacja zostala wylaczona: ...
- Ustawienia prywatnosci przywrocone/pozostawione: ...
- Wynik rejestracji `t.osmanowki@outlook.com` i URL po wyslaniu formularza: ...
```

Za blad blokujacy wdrozenie uznaj co najmniej: brak dostepu do aplikacji, blad 500 w podstawowym przeplywie, mozliwosc obejscia logowania lub izolacji szkol, ujawnienie kontaktu anonimowo, brak wymaganej zgody przy rejestracji, utrate danych formularza lub brak mozliwosci dodania/edycji ogloszenia.