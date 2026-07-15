-- ============================================
-- Pdd.ir Database Schema + Seed Data
-- Created: 2026-07-11
-- ============================================

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200) NULL
    );

    INSERT INTO Roles (Name, Description) VALUES (N'Admin', N'مدیر سیستم');
    INSERT INTO Roles (Name, Description) VALUES (N'User', N'کاربر عادی');
END
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Email NVARCHAR(100) NULL,
        FullName NVARCHAR(100) NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Seed Admin User (password: admin123)
    -- SHA256 hash of "admin123" = JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=
    INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive, CreatedAt)
    VALUES (N'admin', N'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', N'admin@pdd.ir', N'مدیر سیستم', N'Admin', 1, GETUTCDATE());
END
GO

-- RefreshTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(256) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        IsRevoked BIT NOT NULL DEFAULT 0,
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END
GO

-- Products Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Subtitle NVARCHAR(500) NULL,
        Description NVARCHAR(MAX) NULL,
        Features NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        Category NVARCHAR(100) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );

    -- Seed Products
    INSERT INTO Products (Title, Subtitle, Description, Features, ImageUrl, Category, SortOrder, IsActive)
    VALUES (
        N'HIS - سیستم اطلاعات بیمارستانی',
        N'مدیریت جامع بیمارستان و مراکز درمانی',
        N'نرم‌افزار HIS یک سیستم جامع برای مدیریت بیمارستان‌ها و مراکز درمانی است. این سیستم شامل ماژول‌های پذیرش، بخش‌ها، آزمایشگاه، داروخانه، صندوق و گزارش‌گیری پیشرفته می‌باشد.',
        N'["پذیرش و ثبت نام بیمار","مدیریت بخش‌ها و اتاق‌ها","سیستم آزمایشگاه","مدیریت داروخانه","صندوق و حسابداری","گزارش‌ات پیشرفته","رابط کاربری مدرن"]',
        N'/images/his.png',
        N'HIS',
        1,
        1
    );

    INSERT INTO Products (Title, Subtitle, Description, Features, ImageUrl, Category, SortOrder, IsActive)
    VALUES (
        N'نرم‌افزار مالی و حسابداری',
        N'مدیریت مالی جامع برای سازمان‌ها',
        N'نرم‌افزار مالی Pdd یک سیستم حسابداری کامل شامل دفتر کل، حسابهای دریافتنی و پرداختنی، گزارش‌های مالی و مدیریت جریان نقدینگی است.',
        N'["دفتر کل و سرفصل‌ها","حسابهای دریافتنی و پرداختنی","گزارش سود و زیان","ترازنامه","گردش حساب","مدیریت چک","بودجه‌بندی"]',
        N'/images/financial.png',
        N'Financial',
        2,
        1
    );
END
GO

-- ContactMessages Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages')
BEGIN
    CREATE TABLE ContactMessages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Subject NVARCHAR(200) NULL,
        Message NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Pages Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pages')
BEGIN
    CREATE TABLE Pages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Slug NVARCHAR(100) NOT NULL UNIQUE,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NULL,
        MetaDescription NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    INSERT INTO Pages (Slug, Title, Content, MetaDescription, IsActive)
    VALUES (N'about', N'درباره ما', N'شرکت Pdd.ir با بیش از 10 سال تجربه در زمینه توسعه نرم‌افزارهای سازمانی، راهکارهای نوینی برای بیمارستان‌ها و سازمان‌های مالی ارائه می‌دهد.', N'درباره شرکت Pdd.ir', 1);
END
GO

-- SiteSettings Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SiteSettings')
BEGIN
    CREATE TABLE SiteSettings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue NVARCHAR(MAX) NULL,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    INSERT INTO SiteSettings (SettingKey, SettingValue) VALUES (N'CompanyName', N'Pdd.ir');
    INSERT INTO SiteSettings (SettingKey, SettingValue) VALUES (N'Phone', N'021-12345678');
    INSERT INTO SiteSettings (SettingKey, SettingValue) VALUES (N'Email', N'info@pdd.ir');
    INSERT INTO SiteSettings (SettingKey, SettingValue) VALUES (N'Address', N'تهران، خیابان ولیعصر');
END
GO
