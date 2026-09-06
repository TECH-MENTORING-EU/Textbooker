/* =============================================================================
   Textbooker - import danych startowych dla ZSPM (rok szkolny 2026)

   Zakres:
     1. Szkoła: Zespół Szkół Poligraficzno-Mechanicznych w Katowicach
        (domena zspm.pl)
     2. Dane słownikowe: Subjects, Levels, Grades
     3. Książki (Books) wraz z przypisaniem do klas (BookGrades)
        - źródło: docs/books-ZSPM_2006.json

   Uwagi:
     - Skrypt jest idempotentny i można go uruchamiać wielokrotnie.
     - Książki i przypisania klas są synchronizowane do listy źródłowej.
     - Książki spoza listy nie są usuwane, ponieważ mogą mieć ogłoszenia.
     - Uruchamiać na bazie z założonym schematem aplikacji.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------
   1. Szkoła
   --------------------------------------------------------------------------- */
DECLARE @SchoolName nvarchar(200) = N'Zespół Szkół Poligraficzno-Mechanicznych w Katowicach';
DECLARE @SchoolDomain nvarchar(500) = N'zspm.pl';

IF NOT EXISTS (SELECT 1 FROM Schools WHERE LOWER(LTRIM(RTRIM(EmailDomain))) = @SchoolDomain)
BEGIN
    INSERT INTO Schools (Name, EmailDomain, IsActive, CreatedAt)
    VALUES (@SchoolName, @SchoolDomain, 1, SYSUTCDATETIME());
END;

/* ---------------------------------------------------------------------------
   2. Słowniki
   --------------------------------------------------------------------------- */
SET IDENTITY_INSERT Subjects ON;
INSERT INTO Subjects (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES
    (-1, N'Brak'), (1, N'Język polski'), (2, N'Język angielski'),
    (3, N'Język niemiecki'), (4, N'Biologia'), (5, N'Chemia'),
    (6, N'EDB'), (7, N'Fizyka'), (8, N'Geografia'), (9, N'Historia'),
    (10, N'Historia i teraźniejszość'), (11, N'Informatyka'),
    (12, N'Matematyka'), (13, N'Podstawy przedsiębiorczości'),
    (14, N'Biznes i zarządzanie'), (15, N'Plastyka'), (16, N'WOS'),
    (17, N'Język angielski zawodowy'), (18, N'Edukacja obywatelska')
) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM Subjects t WHERE t.Id = v.Id);
SET IDENTITY_INSERT Subjects OFF;

SET IDENTITY_INSERT Levels ON;
INSERT INTO Levels (Id, Name)
SELECT v.Id, v.Name
FROM (VALUES
    (-1, N'Brak'), (1, N'Podstawa'), (2, N'Rozszerzenie'),
    (3, N'Podstawa+Rozszerzenie'), (4, N'Dwujęzyczny')
) AS v(Id, Name)
WHERE NOT EXISTS (SELECT 1 FROM Levels t WHERE t.Id = v.Id);
SET IDENTITY_INSERT Levels OFF;

SET IDENTITY_INSERT Grades ON;
INSERT INTO Grades (Id, GradeNumber)
SELECT v.Id, v.GradeNumber
FROM (VALUES (1, N'1'), (2, N'2'), (3, N'3'), (4, N'4'), (5, N'5')) AS v(Id, GradeNumber)
WHERE NOT EXISTS (SELECT 1 FROM Grades t WHERE t.Id = v.Id);
SET IDENTITY_INSERT Grades OFF;

/* ---------------------------------------------------------------------------
   3. Książki - lista docelowa (50 pozycji)
   --------------------------------------------------------------------------- */
DECLARE @Books TABLE (Id int PRIMARY KEY, Title nvarchar(400) NOT NULL, SubjectId int NOT NULL, LevelId int NOT NULL);

INSERT INTO @Books (Id, Title, SubjectId, LevelId) VALUES
    (-1, N'Inna', -1, -1),
    (1, N'Oblicza epok 1 cz. 1', 1, 3),
    (2, N'Oblicza epok 1 cz. 2', 1, 3),
    (3, N'Oblicza epok 2 cz. 1', 1, 3),
    (4, N'Oblicza epok 2 cz. 2', 1, 3),
    (5, N'Oblicza epok 3 cz. 1', 1, 3),
    (6, N'Oblicza epok 3 cz. 2', 1, 3),
    (7, N'Oblicza epok 4', 1, 3),
    (8, N'Impulse 1 Student''s Book', 2, 3),
    (9, N'Impulse 1 Workbook', 2, 3),
    (10, N'Impulse 2 Student''s Book', 2, 3),
    (11, N'Impulse 2 Workbook', 2, 3),
    (12, N'Impulse 3 Student''s Book', 2, 3),
    (13, N'Repetytorium [Macmillan]', 2, 3),
    (14, N'Repetytorium [Macmillan] poziom rozszerzony', 2, 2),
    (15, N'Ein tolles Team 1', 3, 3),
    (16, N'Ein tolles Team 2', 3, 3),
    (17, N'Ein tolles Team 3', 3, 3),
    (18, N'Podręcznik z repetytorium [Nowa Era]', 3, 3),
    (19, N'Biologia na czasie 2', 4, 1),
    (20, N'Biologia na czasie 3', 4, 1),
    (21, N'Biologia na czasie 3', 4, 2),
    (22, N'Żyję i działam bezpiecznie', 6, 1),
    (23, N'Odkryć fizykę 2', 7, 1),
    (24, N'Odkryć fizykę 3', 7, 1),
    (25, N'NOWE Oblicza geografii 1', 8, 1),
    (26, N'NOWE Oblicza geografii 2', 8, 1),
    (27, N'Oblicza geografii 3', 8, 1),
    (28, N'Geografia 2 [Operon, branżowa]', 8, 1),
    (29, N'Historia 1 [wsip]', 9, 1),
    (30, N'Historia 2 [wsip]', 9, 1),
    (31, N'Historia 3 [wsip]', 9, 1),
    (32, N'Historia 4 [wsip]', 9, 1),
    (33, N'Historia 2 [Operon, branżowa]', 9, 1),
    (34, N'Informatyka 3 [wsip]', 11, 1),
    (35, N'Prosto do matury 1', 12, 1),
    (36, N'Prosto do matury 2', 12, 1),
    (37, N'Prosto do matury 3', 12, 1),
    (38, N'Prosto do matury 4', 12, 1),
    (39, N'Prosto do matury 1', 12, 3),
    (40, N'Prosto do matury 2', 12, 3),
    (41, N'Prosto do matury 4', 12, 3),
    (42, N'NOWA MATeMAtyka 1', 12, 3),
    (43, N'To się liczy! 2 [branżowa]', 12, 1),
    (44, N'Krok w biznes i zarządzanie 1', 14, 1),
    (45, N'Krok w biznes i zarządzanie 2', 14, 1),
    (46, N'Krok w biznes i zarządzanie 1 [branżowa]', 14, 1),
    (47, N'Masz wpływ 1', 18, 1),
    (48, N'Masz wpływ 2', 18, 1),
    (49, N'Bezpieczeństwo i higiena pracy [wsip]', -1, -1);

UPDATE b SET b.Title = s.Title, b.SubjectId = s.SubjectId, b.LevelId = s.LevelId
FROM Books b INNER JOIN @Books s ON s.Id = b.Id
WHERE b.Title <> s.Title OR b.SubjectId <> s.SubjectId OR b.LevelId <> s.LevelId;

SET IDENTITY_INSERT Books ON;
INSERT INTO Books (Id, Title, SubjectId, LevelId)
SELECT s.Id, s.Title, s.SubjectId, s.LevelId
FROM @Books s
WHERE NOT EXISTS (SELECT 1 FROM Books b WHERE b.Id = s.Id);
SET IDENTITY_INSERT Books OFF;

/* ---------------------------------------------------------------------------
   4. Przypisanie książek do klas - stan docelowy (68 powiązań)
   --------------------------------------------------------------------------- */
DECLARE @BookGrades TABLE (BookId int, GradeId int, PRIMARY KEY (BookId, GradeId));

INSERT INTO @BookGrades (BookId, GradeId) VALUES
    (-1,1),(-1,2),(-1,3),(-1,4),(-1,5),(1,1),(2,1),(2,2),(3,2),(4,2),
    (4,3),(5,3),(6,3),(6,4),(6,5),(7,4),(7,5),(8,1),(9,1),(10,2),
    (11,2),(12,3),(12,4),(13,4),(13,5),(14,5),(15,1),(16,2),(17,3),(18,4),
    (18,5),(19,2),(20,3),(20,4),(21,3),(22,1),(23,2),(24,3),(25,1),(26,2),
    (26,3),(27,4),(28,2),(29,1),(30,2),(31,3),(31,4),(32,4),(32,5),(33,2),
    (34,2),(34,3),(35,1),(36,2),(37,3),(38,4),(38,5),(39,1),(40,2),(41,4),
    (42,1),(43,2),(44,1),(45,2),(46,2),(47,2),(48,3),(49,1);

DELETE bg
FROM BookGrades bg
WHERE EXISTS (SELECT 1 FROM @Books b WHERE b.Id = bg.BookId)
  AND NOT EXISTS (SELECT 1 FROM @BookGrades s WHERE s.BookId = bg.BookId AND s.GradeId = bg.GradeId);

INSERT INTO BookGrades (BookId, GradeId)
SELECT s.BookId, s.GradeId
FROM @BookGrades s
WHERE NOT EXISTS (SELECT 1 FROM BookGrades bg WHERE bg.BookId = s.BookId AND bg.GradeId = s.GradeId);

COMMIT TRANSACTION;

/* DBCC CHECKIDENT nie jest operacją transakcyjną, więc wykonuje się po COMMIT. */
DECLARE @table sysname, @maxId int, @sql nvarchar(max);
DECLARE reseed CURSOR LOCAL FAST_FORWARD FOR
   SELECT N'Subjects' UNION ALL
   SELECT N'Levels' UNION ALL
   SELECT N'Grades' UNION ALL
   SELECT N'Books' UNION ALL
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
   END;

   FETCH NEXT FROM reseed INTO @table;
END;
CLOSE reseed;
DEALLOCATE reseed;

/* Książki spoza listy nie są usuwane - mogą mieć powiązane ogłoszenia. */
SELECT b.Id, b.Title, [Ogłoszenia] = (SELECT COUNT(*) FROM Items i WHERE i.BookId = b.Id)
FROM Books b
WHERE NOT EXISTS (SELECT 1 FROM @Books s WHERE s.Id = b.Id)
ORDER BY b.Id;