/* =============================================================================
   Textbooker - import danych startowych dla ŚTZN (rok szkolny 2026)

   Zakres:
     1. Szkoła: Śląskie Techniczne Zakłady Naukowe (domena sltzn.katowice.pl)
     2. Dane słownikowe: Subjects, Levels, Grades
     3. Książki (Books) wraz z przypisaniem do klas (BookGrades)
        - źródło: docs/books_STZN_2026.json

   Uwagi:
     - Skrypt jest idempotentny i można go uruchamiać wielokrotnie.
     - Książki są SYNCHRONIZOWANE, nie tylko dopisywane: rekord o danym Id
       dostaje tytuł/przedmiot/poziom z listy 2026, a przypisania do klas są
       doprowadzane do stanu z listy (brakujące dodawane, nadmiarowe usuwane).
       Jest to konieczne, bo migracje EF zasiewają starszą listę książek
       (86 pozycji, m.in. "Biologia na czasie" zamiast "Nowa Biologia na czasie").
     - Skrypt nie usuwa książek spoza listy - mogą mieć powiązane ogłoszenia
       (Items). Takie pozycje są tylko raportowane na końcu.
     - Identyfikatory są wstawiane jawnie (IDENTITY_INSERT), żeby zgadzały się
       z docs/books_STZN_2026.json oraz z seedem z migracji EF.
     - Uruchamiać na bazie z założonym schematem (po `dotnet ef database update`
       lub po pierwszym starcie aplikacji, która migruje bazę sama).
     - SQL Server. Plik jest w UTF-8 z BOM - dla sqlcmd użyj -f 65001,
       w SSMS/Azure Data Studio zadziała bez flag.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------
   1. Szkoła
   --------------------------------------------------------------------------- */
DECLARE @SchoolName   nvarchar(200) = N'Śląskie Techniczne Zakłady Naukowe';
DECLARE @SchoolDomain nvarchar(500) = N'sltzn.katowice.pl';

IF NOT EXISTS (SELECT 1 FROM Schools WHERE LOWER(LTRIM(RTRIM(EmailDomain))) = @SchoolDomain)
BEGIN
    INSERT INTO Schools (Name, EmailDomain, IsActive, CreatedAt)
    VALUES (@SchoolName, @SchoolDomain, 1, SYSUTCDATETIME());
END
ELSE
    PRINT N'Szkoła o domenie sltzn.katowice.pl już istnieje - pomijam.';

/* ---------------------------------------------------------------------------
   2. Przedmioty
   --------------------------------------------------------------------------- */
SET IDENTITY_INSERT Subjects ON;

INSERT INTO Subjects (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES
    (-1, N'Brak'),
    (1, N'Język polski'),
    (2, N'Język angielski'),
    (3, N'Język niemiecki'),
    (4, N'Biologia'),
    (5, N'Chemia'),
    (6, N'EDB'),
    (7, N'Fizyka'),
    (8, N'Geografia'),
    (9, N'Historia'),
    (10, N'Historia i teraźniejszość'),
    (11, N'Informatyka'),
    (12, N'Matematyka'),
    (13, N'Podstawy przedsiębiorczości'),
    (14, N'Biznes i zarządzanie'),
    (15, N'Plastyka'),
    (16, N'WOS'),
    (17, N'Język angielski zawodowy'),
    (18, N'Edukacja obywatelska')
) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM Subjects t WHERE t.Id = v.Id);

SET IDENTITY_INSERT Subjects OFF;

/* ---------------------------------------------------------------------------
   3. Poziomy nauczania
   --------------------------------------------------------------------------- */
SET IDENTITY_INSERT Levels ON;

INSERT INTO Levels (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES
    (-1, N'Brak'),
    (1, N'Podstawa'),
    (2, N'Rozszerzenie'),
    (3, N'Podstawa+Rozszerzenie'),
    (4, N'Dwujęzyczny')
) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM Levels t WHERE t.Id = v.Id);

SET IDENTITY_INSERT Levels OFF;

/* ---------------------------------------------------------------------------
   4. Klasy
   --------------------------------------------------------------------------- */
SET IDENTITY_INSERT Grades ON;

INSERT INTO Grades (Id, GradeNumber)
SELECT v.Id, v.GradeNumber
FROM (VALUES
    (1, N'1'),
    (2, N'2'),
    (3, N'3'),
    (4, N'4'),
    (5, N'5')
) AS v(Id, GradeNumber)
WHERE NOT EXISTS (SELECT 1 FROM Grades t WHERE t.Id = v.Id);

SET IDENTITY_INSERT Grades OFF;

/* ---------------------------------------------------------------------------
   5. Książki - lista docelowa (93 pozycji)
   --------------------------------------------------------------------------- */
DECLARE @Books TABLE (Id int PRIMARY KEY, Title nvarchar(400) NOT NULL, SubjectId int NOT NULL, LevelId int NOT NULL);

INSERT INTO @Books (Id, Title, SubjectId, LevelId) VALUES
    (  -1, N'Inna',                                                       -1, -1),  -- Brak / Brak
    (   1, N'Ponad słowami 1 cz. 1',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   2, N'Ponad słowami 1 cz. 2',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   3, N'Ponad słowami 2 cz. 1',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   4, N'Ponad słowami 2 cz. 2',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   5, N'Ponad słowami 3 cz. 1',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   6, N'Ponad słowami 3 cz. 2',                                       1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   7, N'Ponad słowami 4',                                             1,  3),  -- Język polski / Podstawa+Rozszerzenie
    (   8, N'Focus 2 Podręcznik',                                          2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (   9, N'Focus 3 Podręcznik',                                          2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  10, N'Focus 4 Podręcznik',                                          2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  11, N'Focus 5 Podręcznik',                                          2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  12, N'Focus 2 Ćwiczenia',                                           2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  13, N'Focus 3 Ćwiczenia',                                           2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  14, N'Focus 4 Ćwiczenia',                                           2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  15, N'Focus 5 Ćwiczenia',                                           2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  16, N'My matura perspectives [nowa era]',                           2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  17, N'Repetytorium [Macmillan]',                                    2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  18, N'Repetytorium maturzysty [Oxford]',                            2,  2),  -- Język angielski / Rozszerzenie
    (  19, N'Repetytorium maturzysty [Cambridge, PWN]',                    2,  3),  -- Język angielski / Podstawa+Rozszerzenie
    (  20, N'Welttour Deutsch neu 1',                                      3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  21, N'Welttour Deutsch neu 2',                                      3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  22, N'Welttour Deutsch neu 3',                                      3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  23, N'Welttour Deutsch 4',                                          3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  24, N'Effekt 1',                                                    3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  25, N'Effekt 2',                                                    3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  26, N'Effekt Neu 3',                                                3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  27, N'Effekt 4',                                                    3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  28, N'Nowa Biologia na czasie 1',                                   4,  1),  -- Biologia / Podstawa
    (  29, N'Nowa Biologia na czasie 2',                                   4,  1),  -- Biologia / Podstawa
    (  30, N'Nowa Biologia na czasie 3',                                   4,  1),  -- Biologia / Podstawa
    (  31, N'Nowa Biologia na czasie 1',                                   4,  2),  -- Biologia / Rozszerzenie
    (  32, N'Nowa Biologia na czasie 2',                                   4,  2),  -- Biologia / Rozszerzenie
    (  33, N'Nowa Biologia na czasie 3',                                   4,  2),  -- Biologia / Rozszerzenie
    (  34, N'Nowa Biologia na czasie 4',                                   4,  2),  -- Biologia / Rozszerzenie
    (  35, N'NOWA To jest chemia 1',                                       5,  1),  -- Chemia / Podstawa
    (  36, N'NOWA To jest chemia 2',                                       5,  1),  -- Chemia / Podstawa
    (  37, N'NOWA To jest chemia 1',                                       5,  2),  -- Chemia / Rozszerzenie
    (  38, N'NOWA To jest chemia 2',                                       5,  2),  -- Chemia / Rozszerzenie
    (  39, N'Edukacja dla bezpieczeństwa. Zakres podstawowy [wsip 2025]',  6,  1),  -- EDB / Podstawa
    (  40, N'Fizyka 1 [wsip]',                                             7,  2),  -- Fizyka / Rozszerzenie
    (  41, N'Fizyka 2 [wsip]',                                             7,  2),  -- Fizyka / Rozszerzenie
    (  42, N'Fizyka 3 [wsip]',                                             7,  2),  -- Fizyka / Rozszerzenie
    (  43, N'Fizyka 4 [wsip]',                                             7,  2),  -- Fizyka / Rozszerzenie
    (  44, N'Fizyka 1 [wsip]',                                             7,  1),  -- Fizyka / Podstawa
    (  45, N'Fizyka 2 [wsip]',                                             7,  1),  -- Fizyka / Podstawa
    (  46, N'Fizyka 3 [wsip]',                                             7,  1),  -- Fizyka / Podstawa
    (  47, N'Fizyka 4 [wsip]',                                             7,  1),  -- Fizyka / Podstawa
    (  48, N'Nowe oblicza geografii 1',                                    8,  1),  -- Geografia / Podstawa
    (  49, N'Nowe oblicza geografii 2',                                    8,  1),  -- Geografia / Podstawa
    (  50, N'Nowe oblicza geografii karty pracy 1',                        8,  1),  -- Geografia / Podstawa
    (  51, N'Nowe oblicza geografii karty pracy 2',                        8,  1),  -- Geografia / Podstawa
    (  52, N'Historia 1. Zakres podstawowy [wsip]',                        9,  1),  -- Historia / Podstawa
    (  53, N'Historia 2. Zakres podstawowy [wsip]',                        9,  1),  -- Historia / Podstawa
    (  54, N'Historia 3. Zakres podstawowy [wsip]',                        9,  1),  -- Historia / Podstawa
    (  55, N'Historia 4. Zakres podstawowy [wsip]',                        9,  1),  -- Historia / Podstawa
    (  56, N'Historia i teraźniejszość [wsip] 1',                         10,  1),  -- Historia i teraźniejszość / Podstawa
    (  57, N'Historia i teraźniejszość [wsip] 2',                         10,  1),  -- Historia i teraźniejszość / Podstawa
    (  58, N'Informatyka [operon]',                                       11,  1),  -- Informatyka / Podstawa
    (  59, N'Informatyka dla szkół ponadgimnazjalnych [Migra]',           11,  1),  -- Informatyka / Podstawa
    (  60, N'Informatyka [operon]',                                       11,  2),  -- Informatyka / Rozszerzenie
    (  61, N'Informatyka dla szkół ponadgimnazjalnych [Migra]',           11,  2),  -- Informatyka / Rozszerzenie
    (  62, N'NOWA MATeMAtyka 1',                                          12,  1),  -- Matematyka / Podstawa
    (  63, N'NOWA MATeMAtyka 2',                                          12,  1),  -- Matematyka / Podstawa
    (  64, N'NOWA MATeMAtyka 3',                                          12,  1),  -- Matematyka / Podstawa
    (  65, N'NOWA MATeMAtyka 4',                                          12,  1),  -- Matematyka / Podstawa
    (  66, N'NOWA MATeMAtyka 1',                                          12,  3),  -- Matematyka / Podstawa+Rozszerzenie
    (  67, N'NOWA MATeMAtyka 2',                                          12,  3),  -- Matematyka / Podstawa+Rozszerzenie
    (  68, N'NOWA MATeMAtyka 3',                                          12,  3),  -- Matematyka / Podstawa+Rozszerzenie
    (  69, N'NOWA MATeMAtyka 4',                                          12,  3),  -- Matematyka / Podstawa+Rozszerzenie
    (  70, N'Krok w przedsiębiorczość',                                   13,  1),  -- Podstawy przedsiębiorczości / Podstawa
    (  71, N'Krok w biznes i zarządzanie 1',                              14,  1),  -- Biznes i zarządzanie / Podstawa
    (  72, N'Krok w biznes i zarządzanie 2',                              14,  1),  -- Biznes i zarządzanie / Podstawa
    (  73, N'Spotkania ze sztuką 1',                                      15,  1),  -- Plastyka / Podstawa
    (  74, N'Masz wpływ 1',                                               18,  1),  -- Edukacja obywatelska / Podstawa
    (  75, N'W centrum uwagi 1',                                          16,  1),  -- WOS / Podstawa
    (  76, N'W centrum uwagi 2',                                          16,  1),  -- WOS / Podstawa
    (  77, N'Electronics',                                                17, -1),  -- Język angielski zawodowy / Brak
    (  78, N'Electrician',                                                17, -1),  -- Język angielski zawodowy / Brak
    (  79, N'Software engineering',                                       17, -1),  -- Język angielski zawodowy / Brak
    (  80, N'Computing',                                                  17, -1),  -- Język angielski zawodowy / Brak
    (  81, N'Mechanical engineering',                                     17, -1),  -- Język angielski zawodowy / Brak
    (  82, N'Mechanics',                                                  17, -1),  -- Język angielski zawodowy / Brak
    (  83, N'Environmental Science',                                      17, -1),  -- Język angielski zawodowy / Brak
    (  84, N'IT [english for IT]',                                        17, -1),  -- Język angielski zawodowy / Brak
    (  85, N'Informatyka w praktyce',                                     11,  2),  -- Informatyka / Rozszerzenie
    (  86, N'#trends neu 1',                                               3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  87, N'#trends neu 2',                                               3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  88, N'#trends neu 3',                                               3,  3),  -- Język niemiecki / Podstawa+Rozszerzenie
    (  89, N'NOWA To jest chemia 3',                                       5,  1),  -- Chemia / Podstawa
    (  90, N'NOWA To jest chemia. Zbiór zadań',                            5,  2),  -- Chemia / Rozszerzenie
    (  91, N'Masz wpływ 2',                                               18,  1),  -- Edukacja obywatelska / Podstawa
    (  92, N'Repetytorium maturalne [Pearson]',                            2,  3);  -- Język angielski / Podstawa+Rozszerzenie

-- 5a. Aktualizacja pozycji, które już są w bazie (np. seed z migracji EF)
UPDATE b
SET b.Title = s.Title, b.SubjectId = s.SubjectId, b.LevelId = s.LevelId
FROM Books b
INNER JOIN @Books s ON s.Id = b.Id
WHERE b.Title <> s.Title OR b.SubjectId <> s.SubjectId OR b.LevelId <> s.LevelId;

PRINT CONCAT(N'Zaktualizowane książki: ', @@ROWCOUNT);

-- 5b. Wstawienie brakujących pozycji
SET IDENTITY_INSERT Books ON;

INSERT INTO Books (Id, Title, SubjectId, LevelId)
SELECT s.Id, s.Title, s.SubjectId, s.LevelId
FROM @Books s
WHERE NOT EXISTS (SELECT 1 FROM Books b WHERE b.Id = s.Id);

PRINT CONCAT(N'Dodane książki: ', @@ROWCOUNT);

SET IDENTITY_INSERT Books OFF;

/* ---------------------------------------------------------------------------
   6. Przypisanie książek do klas - stan docelowy (160 powiązań)
   --------------------------------------------------------------------------- */
DECLARE @BookGrades TABLE (BookId int, GradeId int, PRIMARY KEY (BookId, GradeId));

INSERT INTO @BookGrades (BookId, GradeId) VALUES
    (  -1, 1),
    (  -1, 2),
    (  -1, 3),
    (  -1, 4),
    (  -1, 5),
    (   1, 1),
    (   2, 1),
    (   3, 2),
    (   4, 2),
    (   4, 3),
    (   5, 3),
    (   6, 4),
    (   7, 5),
    (   8, 1),
    (   8, 2),
    (   8, 3),
    (   9, 1),
    (   9, 2),
    (   9, 3),
    (  10, 1),
    (  10, 2),
    (  10, 3),
    (  10, 4),
    (  11, 3),
    (  11, 4),
    (  11, 5),
    (  12, 1),
    (  12, 2),
    (  12, 3),
    (  13, 1),
    (  13, 2),
    (  13, 3),
    (  14, 1),
    (  14, 2),
    (  14, 3),
    (  14, 4),
    (  15, 3),
    (  15, 4),
    (  15, 5),
    (  16, 4),
    (  16, 5),
    (  17, 5),
    (  18, 5),
    (  19, 5),
    (  20, 1),
    (  21, 1),
    (  21, 2),
    (  22, 3),
    (  23, 4),
    (  23, 5),
    (  24, 1),
    (  24, 2),
    (  25, 2),
    (  25, 3),
    (  26, 4),
    (  26, 5),
    (  27, 4),
    (  27, 5),
    (  28, 1),
    (  29, 2),
    (  29, 3),
    (  30, 4),
    (  31, 1),
    (  32, 2),
    (  33, 4),
    (  34, 5),
    (  35, 1),
    (  36, 2),
    (  37, 1),
    (  37, 2),
    (  37, 3),
    (  38, 4),
    (  38, 5),
    (  39, 1),
    (  40, 1),
    (  41, 2),
    (  42, 3),
    (  43, 4),
    (  43, 5),
    (  44, 1),
    (  45, 2),
    (  46, 3),
    (  46, 4),
    (  47, 4),
    (  47, 5),
    (  48, 2),
    (  48, 4),
    (  49, 3),
    (  50, 2),
    (  50, 4),
    (  51, 3),
    (  52, 1),
    (  53, 2),
    (  54, 3),
    (  55, 4),
    (  55, 5),
    (  56, 2),
    (  57, 3),
    (  58, 1),
    (  58, 2),
    (  59, 2),
    (  59, 3),
    (  59, 4),
    (  60, 1),
    (  60, 2),
    (  61, 2),
    (  61, 3),
    (  61, 4),
    (  62, 1),
    (  62, 2),
    (  63, 2),
    (  63, 3),
    (  64, 3),
    (  64, 4),
    (  65, 4),
    (  65, 5),
    (  66, 1),
    (  66, 2),
    (  67, 2),
    (  67, 3),
    (  68, 3),
    (  68, 4),
    (  69, 4),
    (  69, 5),
    (  70, 2),
    (  71, 1),
    (  72, 2),
    (  73, 1),
    (  74, 2),
    (  75, 4),
    (  75, 5),
    (  76, 4),
    (  76, 5),
    (  77, 3),
    (  77, 4),
    (  78, 3),
    (  78, 4),
    (  79, 3),
    (  79, 4),
    (  80, 3),
    (  80, 4),
    (  81, 3),
    (  81, 4),
    (  82, 3),
    (  82, 4),
    (  83, 3),
    (  83, 4),
    (  84, 3),
    (  84, 4),
    (  85, 3),
    (  86, 1),
    (  87, 2),
    (  88, 3),
    (  89, 3),
    (  89, 4),
    (  90, 1),
    (  90, 2),
    (  90, 3),
    (  91, 3),
    (  92, 5);

-- 6a. Usunięcie przypisań, których nie ma na liście 2026
--     (tylko dla książek z listy - reszty tabeli nie ruszamy)
DELETE bg
FROM BookGrades bg
WHERE EXISTS (SELECT 1 FROM @Books b WHERE b.Id = bg.BookId)
  AND NOT EXISTS (SELECT 1 FROM @BookGrades s WHERE s.BookId = bg.BookId AND s.GradeId = bg.GradeId);

PRINT CONCAT(N'Usunięte przypisania do klas: ', @@ROWCOUNT);

-- 6b. Dodanie brakujących przypisań
INSERT INTO BookGrades (BookId, GradeId)
SELECT s.BookId, s.GradeId
FROM @BookGrades s
WHERE NOT EXISTS (SELECT 1 FROM BookGrades bg WHERE bg.BookId = s.BookId AND bg.GradeId = s.GradeId);

PRINT CONCAT(N'Dodane przypisania do klas: ', @@ROWCOUNT);

COMMIT TRANSACTION;

/* ---------------------------------------------------------------------------
   7. Przestawienie liczników IDENTITY na MAX(Id)

   DBCC CHECKIDENT nie jest operacją transakcyjną, dlatego wykonuje się
   po zatwierdzeniu transakcji.
   --------------------------------------------------------------------------- */
DECLARE @table sysname, @maxId int, @sql nvarchar(max);
DECLARE reseed CURSOR LOCAL FAST_FORWARD FOR
    SELECT N'Subjects' UNION ALL
    SELECT N'Levels'   UNION ALL
    SELECT N'Grades'   UNION ALL
    SELECT N'Books'    UNION ALL
    SELECT N'Schools';

OPEN reseed;
FETCH NEXT FROM reseed INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT @m = ISNULL(MAX(Id), 0) FROM ' + @table + N';';
    EXEC sp_executesql @sql, N'@m int OUTPUT', @m = @maxId OUTPUT;

    IF @maxId > 0
    BEGIN
        SET @sql = N'DBCC CHECKIDENT (' + CHAR(39) + @table + CHAR(39) + N', RESEED, '
                 + CAST(@maxId AS nvarchar(20)) + N') WITH NO_INFOMSGS;';
        EXEC sp_executesql @sql;
    END

    FETCH NEXT FROM reseed INTO @table;
END
CLOSE reseed;
DEALLOCATE reseed;

/* ---------------------------------------------------------------------------
   8. Podsumowanie
   --------------------------------------------------------------------------- */
SELECT 'Schools' AS TableName, COUNT(*) AS [Rows] FROM Schools
UNION ALL SELECT 'Subjects',   COUNT(*) FROM Subjects
UNION ALL SELECT 'Levels',     COUNT(*) FROM Levels
UNION ALL SELECT 'Grades',     COUNT(*) FROM Grades
UNION ALL SELECT 'Books',      COUNT(*) FROM Books
UNION ALL SELECT 'BookGrades', COUNT(*) FROM BookGrades;

-- Książki w bazie spoza listy 2026 (nie są usuwane - mogą mieć ogłoszenia)
SELECT b.Id, b.Title, [Ogłoszenia] = (SELECT COUNT(*) FROM Items i WHERE i.BookId = b.Id)
FROM Books b
WHERE NOT EXISTS (SELECT 1 FROM @Books s WHERE s.Id = b.Id)
ORDER BY b.Id;
