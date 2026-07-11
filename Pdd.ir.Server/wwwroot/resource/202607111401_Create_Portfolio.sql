-- Portfolio Items Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PortfolioItems')
BEGIN
    CREATE TABLE PortfolioItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        ProjectUrl NVARCHAR(500) NULL,
        Category NVARCHAR(100) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    INSERT INTO PortfolioItems (Title, Description, ImageUrl, Category, SortOrder, IsActive)
    VALUES (N'بیمارستان مرکزی تهران', N'پیاده‌سازی کامل سیستم HIS شامل پذیرش، بخش‌ها، آزمایشگاه و داروخانه', N'/images/portfolio/hospital.jpg', N'HIS', 1, 1);

    INSERT INTO PortfolioItems (Title, Description, ImageUrl, Category, SortOrder, IsActive)
    VALUES (N'شرکت مالی آریا', N'پیاده‌سازی نرم‌افزار حسابداری و مدیریت مالی جامع', N'/images/portfolio/financial.jpg', N'Financial', 2, 1);

    INSERT INTO PortfolioItems (Title, Description, ImageUrl, Category, SortOrder, IsActive)
    VALUES (N'کلینیک تخصصی پارسیان', N'سیستم مدیریت نوبت‌دهی و پرونده الکترونیک بیمار', N'/images/portfolio/clinic.jpg', N'HIS', 3, 1);
END
GO
