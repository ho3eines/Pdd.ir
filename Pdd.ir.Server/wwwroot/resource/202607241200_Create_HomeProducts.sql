-- ============================================
-- HomeProducts table - home page products panel
-- ============================================

IF OBJECT_ID('HomeProducts', 'U') IS NULL
BEGIN
    CREATE TABLE HomeProducts (
        Id INT PRIMARY KEY IDENTITY(1,1),
        NameFa NVARCHAR(200) NOT NULL DEFAULT '',
        NameEn NVARCHAR(200) NOT NULL DEFAULT '',
        SubFa NVARCHAR(500) NOT NULL DEFAULT '',
        SubEn NVARCHAR(500) NOT NULL DEFAULT '',
        Icon NVARCHAR(100) NOT NULL DEFAULT 'bi-box',
        IconBg NVARCHAR(50) NOT NULL DEFAULT 'rgba(13,110,253,0.15)',
        IconColor NVARCHAR(20) NOT NULL DEFAULT '#0D6EFD',
        Route NVARCHAR(200) NOT NULL DEFAULT '/products',
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
    
    CREATE INDEX IX_HomeProducts_SortOrder ON HomeProducts(SortOrder);
    CREATE INDEX IX_HomeProducts_IsActive ON HomeProducts(IsActive);
END
GO

-- Insert default data only if empty
IF NOT EXISTS (SELECT 1 FROM HomeProducts)
BEGIN
    DECLARE @Now BIGINT = DATEDIFF(second, '1970-01-01', GETUTCDATE());
    
    INSERT INTO HomeProducts (NameFa, NameEn, SubFa, SubEn, Icon, IconBg, IconColor, Route, SortOrder, IsActive, CreatedAt) VALUES
    ('HIS — بیمارستانی', 'HIS — Hospital', 'مدیریت جامع بیمارستان', 'Hospital Management', 'bi-hospital', 'rgba(13,110,253,0.15)', '#0D6EFD', '/products', 1, 1, @Now),
    ('CIS — درمانگاهی', 'CIS — Clinic', 'مدیریت کلینیک', 'Clinic Management', 'bi-clipboard2-pulse', 'rgba(248,28,229,0.15)', '#f81ce5', '/products', 2, 1, @Now),
    ('RIS — تصویربرداری', 'RIS — Imaging', 'مراکز MRI و CT', 'MRI & CT Centers', 'bi-camera-reels', 'rgba(121,40,202,0.15)', '#7928ca', '/products', 3, 1, @Now),
    ('LIS — آزمایشگاه', 'LIS — Laboratory', 'مدیریت آزمایشگاه', 'Lab Management', 'bi-lungs', 'rgba(80,227,194,0.15)', '#50e3c2', '/products', 4, 1, @Now),
    ('MIS — مالی اداری', 'MIS — Financial', 'حسابداری و خزانه', 'Accounting & Treasury', 'bi-calculator', 'rgba(13,110,253,0.15)', '#0D6EFD', '/products', 5, 1, @Now),
    ('اتوماسیون اداری', 'Office Automation', 'گردش نامه و بایگانی', 'Document & Filing', 'bi-building', 'rgba(248,28,229,0.15)', '#f81ce5', '/products', 6, 1, @Now);
END
GO
