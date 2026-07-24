-- ============================================
-- Clients table - clean single migration
-- Only creates if not exists, never drops
-- ============================================

IF OBJECT_ID('Clients', 'U') IS NULL
BEGIN
    CREATE TABLE Clients (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        NameEn NVARCHAR(200) NOT NULL DEFAULT '',
        ImageUrl NVARCHAR(500) NOT NULL DEFAULT '',
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
    
    CREATE INDEX IX_Clients_SortOrder ON Clients(SortOrder);
    CREATE INDEX IX_Clients_IsActive ON Clients(IsActive);
END
GO

-- Ensure ImageUrl column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE Clients ADD ImageUrl NVARCHAR(500) NOT NULL DEFAULT '';
END
GO

-- Insert default data only if empty
IF NOT EXISTS (SELECT 1 FROM Clients)
BEGIN
    DECLARE @Now BIGINT = DATEDIFF(second, '1970-01-01', GETUTCDATE());
    
    INSERT INTO Clients (Name, NameEn, ImageUrl, SortOrder, IsActive, CreatedAt) VALUES
    ('شهید بهشتی', 'Shahid Beheshti', '', 1, 1, @Now),
    ('دانشگاه تهران', 'TUMS', '', 2, 1, @Now),
    ('دانشگاه ایران', 'Iran University', '', 3, 1, @Now),
    ('قائم جیرفت', 'Ghaem Jiroft', '', 4, 1, @Now),
    ('محمد کرمانشاهی', 'Mohammad Kermanshahi', '', 5, 1, @Now),
    ('شفا ساری', 'Shafa Sari', '', 6, 1, @Now),
    ('حضرت فاطمه ساوه', 'Hazrate Fatemeh Saveh', '', 7, 1, @Now),
    ('آیت‌الله گلپایگانی', 'Ayatollah Golpaygani', '', 8, 1, @Now),
    ('امیرالمؤمنین', 'Amiralmomenin', '', 9, 1, @Now),
    ('امام خمینی', 'Imam Khomeini', '', 10, 1, @Now),
    ('دانشگاه گلستان', 'Golestan University', '', 11, 1, @Now),
    ('دانشگاه سمنان', 'Semnan University', '', 12, 1, @Now),
    ('دانشگاه زنجان', 'Zanjan University', '', 13, 1, @Now),
    ('دانشگاه یزد', 'Yazd University', '', 14, 1, @Now),
    ('دانشگاه کاشان', 'Kashan University', '', 15, 1, @Now),
    ('دانشگاه قزوین', 'Qazvin University', '', 16, 1, @Now);
END
GO
