/* =============================================================================
   Textbooker - full SQL Server database cleanup

   WARNING: this script irreversibly deletes ALL user tables in the given
   schema, including users, listings, school data and __EFMigrationsHistory.
   After it runs, the application will recreate the schema on the next EF Core
   migration run.

   The committed version of this script intentionally does NOT contain database
   or schema names. Values are set in the DECLARE block below (marked REMOVE
   BEFORE COMMIT).

   ============================================================================= */

/* >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
   LOCAL VALUES — REMOVE THIS BLOCK BEFORE COMMIT!
   (plain T-SQL — works in SSMS without SQLCMD mode)
   <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< */
DECLARE @ExpectedDatabase sysname = N'';
DECLARE @TargetSchema sysname = N'';

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* Dynamic USE — a static USE does not accept variables. The rest of the script
   then runs in the target database (DB_NAME() reflects the new context). */
DECLARE @UseSql nvarchar(max) = N'USE ' + QUOTENAME(@ExpectedDatabase) + N';';
EXEC sys.sp_executesql @UseSql;

IF DB_NAME() <> @ExpectedDatabase
    THROW 50000, 'Skrypt uruchomiono na niewłaściwej bazie danych.', 1;

IF DB_NAME() = N'master'
    THROW 50001, 'Nie wolno uruchamiać skryptu na bazie master.', 1;

BEGIN TRANSACTION;

DECLARE @Sql nvarchar(max) = N'';

/* Drop all foreign keys first, so DROP TABLE order does not matter. */
SELECT @Sql = @Sql + N'ALTER TABLE '
    + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.'
    + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
WHERE OBJECT_SCHEMA_NAME(fk.parent_object_id) = @TargetSchema;

IF @Sql <> N''
    EXEC sys.sp_executesql @Sql;

SET @Sql = N'';

SELECT @Sql = @Sql + N'DROP TABLE '
    + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
    + N';' + CHAR(13) + CHAR(10)
FROM sys.tables t
WHERE t.is_ms_shipped = 0
    AND SCHEMA_NAME(t.schema_id) = @TargetSchema;

IF @Sql <> N''
    EXEC sys.sp_executesql @Sql;

COMMIT TRANSACTION;

PRINT N'Baza została wyczyszczona. Uruchom migracje EF Core, aby odtworzyć schemat.';