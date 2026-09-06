/* =============================================================================
   Textbooker - grant the "Admin" role to existing accounts

   Looks up users by e-mail (case-insensitive, via NormalizedEmail) and adds
   them to the "Admin" role. The role is created if it does not exist yet
   (mirrors StartupUtilities.InitializeRolesAsync).

   Users are NOT created here - the script only touches existing accounts.
   Missing e-mails are reported as a warning and do not abort the script.

   The committed version of this script intentionally does NOT contain database
   or schema names. Values are set in the DECLARE block below (marked REMOVE
   BEFORE COMMIT).

   Idempotent: re-running skips role assignments that already exist.

   ============================================================================= */

/* >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
   LOCAL VALUES — REMOVE THIS BLOCK BEFORE COMMIT!
   (plain T-SQL — works in SSMS without SQLCMD mode)
   <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< */
DECLARE @ExpectedDatabase sysname = N'';
DECLARE @TargetSchema sysname = N'';

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Dynamic USE — a static USE does not accept variables. */
DECLARE @UseSql nvarchar(max) = N'USE ' + QUOTENAME(@ExpectedDatabase) + N';';
EXEC sys.sp_executesql @UseSql;

IF DB_NAME() <> @ExpectedDatabase
    THROW 50000, 'Skrypt uruchomiono na niewłaściwej bazie danych.', 1;

/* ---------------------------------------------------------------------------
   E-mails to promote (matched against NormalizedEmail, case-insensitive).
   A temp table is used because it stays visible inside sp_executesql.
   --------------------------------------------------------------------------- */
CREATE TABLE #Emails (Email nvarchar(256) NOT NULL PRIMARY KEY);
INSERT INTO #Emails (Email) VALUES
    (N't.osmanowski@gmail.com'),
    (N't.osmanowski@outlook.com');

DECLARE @SchemaPrefix nvarchar(300) = QUOTENAME(@TargetSchema) + N'.';
DECLARE @RoleTable nvarchar(300) = @SchemaPrefix + N'[AspNetRoles]';
DECLARE @UserTable nvarchar(300) = @SchemaPrefix + N'[AspNetUsers]';
DECLARE @UserRoleTable nvarchar(300) = @SchemaPrefix + N'[AspNetUserRoles]';

IF OBJECT_ID(@RoleTable, N'U') IS NULL OR OBJECT_ID(@UserTable, N'U') IS NULL OR OBJECT_ID(@UserRoleTable, N'U') IS NULL
    THROW 50010, N'Tabele Identity nie istnieją - uruchom najpierw migracje EF Core.', 1;

BEGIN TRANSACTION;

/* 1. Ensure the "Admin" role exists. */
DECLARE @InsertRole nvarchar(max) = N'
IF NOT EXISTS (SELECT 1 FROM ' + @RoleTable + N' WHERE [NormalizedName] = N''ADMIN'')
    INSERT INTO ' + @RoleTable + N' ([Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (N''Admin'', N''ADMIN'', LOWER(CONVERT(nvarchar(36), NEWID())));';
EXEC sys.sp_executesql @InsertRole;

DECLARE @AdminRoleId int;
DECLARE @GetRoleId nvarchar(max) = N'SELECT @RoleIdOut = [Id] FROM ' + @RoleTable + N' WHERE [NormalizedName] = N''ADMIN'';';
EXEC sys.sp_executesql @GetRoleId, N'@RoleIdOut int OUTPUT', @RoleIdOut = @AdminRoleId OUTPUT;

IF @AdminRoleId IS NULL
    THROW 50011, N'Nie udało się utworzyć/odczytać roli Admin.', 1;

/* 2. Assign the role to every matching user (skip existing assignments). */
DECLARE @Assign nvarchar(max) = N'
INSERT INTO ' + @UserRoleTable + N' ([UserId], [RoleId])
SELECT u.[Id], @RoleId
FROM ' + @UserTable + N' u
INNER JOIN #Emails e ON u.[NormalizedEmail] = UPPER(e.[Email])
WHERE NOT EXISTS (
    SELECT 1 FROM ' + @UserRoleTable + N' ur
    WHERE ur.[UserId] = u.[Id] AND ur.[RoleId] = @RoleId);';
EXEC sys.sp_executesql @Assign, N'@RoleId int', @RoleId = @AdminRoleId;

COMMIT TRANSACTION;

/* 3. Report: which e-mails matched a user and their final admin status. */
DECLARE @Report nvarchar(max) = N'
SELECT e.[Email],
       [MatchedUser] = u.[UserName],
       [IsAdmin] = CASE WHEN EXISTS (
           SELECT 1 FROM ' + @UserRoleTable + N' ur
           WHERE ur.[UserId] = u.[Id] AND ur.[RoleId] = @RoleId
       ) THEN 1 ELSE 0 END
FROM #Emails e
LEFT JOIN ' + @UserTable + N' u ON u.[NormalizedEmail] = UPPER(e.[Email]);';
EXEC sys.sp_executesql @Report, N'@RoleId int', @RoleId = @AdminRoleId;

/* FOR XML PATH used instead of STRING_AGG for compatibility with SQL Server < 2017. */
DECLARE @Missing nvarchar(max) = N'
DECLARE @MissingEmails nvarchar(max) = STUFF((
    SELECT N'', '' + e.[Email]
    FROM #Emails e
    WHERE NOT EXISTS (
        SELECT 1 FROM ' + @UserTable + N' u
        WHERE u.[NormalizedEmail] = UPPER(e.[Email]))
    FOR XML PATH(N''''), TYPE).value(N''.'', N''nvarchar(max)''), 1, 2, N'''');

IF @MissingEmails IS NOT NULL
    PRINT N''UWAGA: nie znaleziono użytkowników dla adresów: '' + @MissingEmails;
ELSE
    PRINT N''Wszyscy wskazani użytkownicy mają rolę Admin.'';
';
EXEC sys.sp_executesql @Missing;

DROP TABLE #Emails;
