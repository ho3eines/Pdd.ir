-- ============================================
-- FINAL: Create Clients table with ImageData
-- Safe to run multiple times
-- ============================================

-- Step 1: Clean up old migration records
DELETE FROM ResurceExecute WHERE FileName LIKE '%Clients%';
GO

-- Step 2: Create table if not exists
IF OBJECT_ID('Clients', 'U') IS NULL
BEGIN
    CREATE TABLE Clients (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        NameEn NVARCHAR(200) NOT NULL DEFAULT '',
        ImageData VARBINARY(MAX) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
    
    CREATE INDEX IX_Clients_SortOrder ON Clients(SortOrder);
    CREATE INDEX IX_Clients_IsActive ON Clients(IsActive);
    
    PRINT '✅ Clients table created';
END
GO

-- Step 3: Ensure ImageData column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'ImageData')
BEGIN
    ALTER TABLE Clients ADD ImageData VARBINARY(MAX) NULL;
    PRINT '✅ ImageData column added';
END
GO

-- Step 4: Remove old columns if they exist
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'Icon')
BEGIN
    ALTER TABLE Clients DROP COLUMN Icon;
    PRINT '✅ Icon column removed';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'Color')
BEGIN
    ALTER TABLE Clients DROP COLUMN Color;
    PRINT '✅ Color column removed';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE Clients DROP COLUMN ImageUrl;
    PRINT '✅ ImageUrl column removed';
END
GO

-- Step 5: Insert default data if empty
IF NOT EXISTS (SELECT 1 FROM Clients)
BEGIN
    DECLARE @Now BIGINT = DATEDIFF(second, '1970-01-01', GETUTCDATE());
    
    INSERT INTO Clients (Name, NameEn, ImageData, SortOrder, IsActive, CreatedAt) VALUES
    ('شهید بهشتی', 'Shahid Beheshti', NULL, 1, 1, @Now),
    ('دانشگاه تهران', 'TUMS', NULL, 2, 1, @Now),
    ('دانشگاه ایران', 'Iran University', NULL, 3, 1, @Now),
    ('قائم جیرفت', 'Ghaem Jiroft', NULL, 4, 1, @Now),
    ('محمد کرمانشاهی', 'Mohammad Kermanshahi', NULL, 5, 1, @Now),
    ('شفا ساری', 'Shafa Sari', NULL, 6, 1, @Now),
    ('حضرت فاطمه ساوه', 'Hazrate Fatemeh Saveh', NULL, 7, 1, @Now),
    ('آیت‌الله گلپایگانی', 'Ayatollah Golpaygani', NULL, 8, 1, @Now),
    ('امیرالمؤمنین', 'Amiralmomenin', NULL, 9, 1, @Now),
    ('امام خمینی', 'Imam Khomeini', NULL, 10, 1, @Now),
    ('دانشگاه گلستان', 'Golestan University', NULL, 11, 1, @Now),
    ('دانشگاه سمنان', 'Semnan University', NULL, 12, 1, @Now),
    ('دانشگاه زنجان', 'Zanjan University', NULL, 13, 1, @Now),
    ('دانشگاه یزد', 'Yazd University', NULL, 14, 1, @Now),
    ('دانشگاه کاشان', 'Kashan University', NULL, 15, 1, @Now),
    ('دانشگاه قزوین', 'Qazvin University', NULL, 16, 1, @Now);
    
    PRINT '✅ Default data inserted';
END
GO

-- Step 6: Verify
PRINT '========================================';
PRINT 'VERIFICATION:';
PRINT '========================================';

SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Clients')
ORDER BY c.column_id;

SELECT COUNT(*) AS TotalClients FROM Clients;
PRINT '========================================';
