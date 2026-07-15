-- Create Database (tables are created by ScriptExecutor on app startup)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'pdd')
BEGIN
    CREATE DATABASE pdd;
END
GO
