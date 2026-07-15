-- ============================================
-- PDD.ir — Docker Init Script
-- Creates database and runs all migrations
-- ============================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'pdd')
BEGIN
    CREATE DATABASE pdd;
END
GO

USE pdd;
GO

-- ============================================
-- 1. Roles Table
-- ============================================
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

-- ============================================
-- 2. Users Table
-- ============================================
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
    -- Admin user (password: admin123)
    INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive, CreatedAt)
    VALUES (N'admin', N'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', N'admin@pdd.ir', N'مدیر سیستم', N'Admin', 1, GETUTCDATE());
END
GO

-- ============================================
-- 3. RefreshTokens Table
-- ============================================
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

-- ============================================
-- 4. Products Table
-- ============================================
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

    INSERT INTO Products (Title, Subtitle, Description, Features, ImageUrl, Category, SortOrder, IsActive)
    VALUES (
        N'HIS - سیستم اطلاعات بیمارستانی',
        N'مدیریت جامع بیمارستان و مراکز درمانی',
        N'نرم‌افزار HIS یک سیستم جامع برای مدیریت بیمارستان‌ها و مراکز درمانی است.',
        N'["پذیرش و ثبت نام بیمار","مدیریت بخش‌ها و اتاق‌ها","سیستم آزمایشگاه","مدیریت داروخانه"]',
        N'/images/his.png', N'HIS', 1, 1
    );

    INSERT INTO Products (Title, Subtitle, Description, Features, ImageUrl, Category, SortOrder, IsActive)
    VALUES (
        N'نرم‌افزار مالی و حسابداری',
        N'مدیریت مالی جامع برای سازمان‌ها',
        N'نرم‌افزار مالی Pdd یک سیستم حسابداری کامل شامل دفتر کل و گزارش‌های مالی.',
        N'["دفتر کل و سرفصل‌ها","حسابهای دریافتنی و پرداختنی","گزارش سود و زیان","ترازنامه"]',
        N'/images/financial.png', N'Financial', 2, 1
    );
END
GO

-- ============================================
-- 5. ContactMessages Table
-- ============================================
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

-- ============================================
-- 6. Pages Table
-- ============================================
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
    VALUES (N'about', N'درباره ما', N'شرکت Pdd.ir با بیش از 10 سال تجربه در زمینه توسعه نرم‌افزارهای سازمانی.', N'درباره شرکت Pdd.ir', 1);
END
GO

-- ============================================
-- 7. SiteSettings Table
-- ============================================
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

-- ============================================
-- 8. BlogPosts Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogPosts')
BEGIN
    CREATE TABLE BlogPosts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(200) NOT NULL UNIQUE,
        Summary NVARCHAR(500) NULL,
        Content NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        Author NVARCHAR(100) NULL,
        IsPublished BIT NOT NULL DEFAULT 0,
        ViewCount INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    INSERT INTO BlogPosts (Title, Slug, Summary, Content, Author, IsPublished)
    VALUES (N'معرفی سیستم HIS', N'his-introduction', N'آشنایی با سیستم اطلاعات بیمارستانی', N'سیستم HIS یک راهکار جامع برای مدیریت بیمارستان‌هاست.', N'Admin', 1);
END
GO

-- ============================================
-- 9. PortfolioItems Table
-- ============================================
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
    VALUES (N'بیمارستان مرکزی تهران', N'پیاده‌سازی کامل سیستم HIS', N'/images/portfolio/hospital.jpg', N'HIS', 1, 1);
END
GO

-- ============================================
-- 10. Permissions Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE Permissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Label NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General'
    );
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.view', N'مشاهده محصولات', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.create', N'ایجاد محصول', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.edit', N'ویرایش محصول', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.delete', N'حذف محصول', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.view', N'مشاهده بلاگ', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.create', N'ایجاد مقاله', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.edit', N'ویرایش مقاله', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.delete', N'حذف مقاله', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.view', N'مشاهده نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.create', N'ایجاد نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.edit', N'ویرایش نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.delete', N'حذف نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'contact.view', N'مشاهده پیام‌ها', N'Contact');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'contact.delete', N'حذف پیام', N'Contact');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.view', N'مشاهده صفحات', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.create', N'ایجاد صفحه', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.edit', N'ویرایش صفحه', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.delete', N'حذف صفحه', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.view', N'مشاهده کاربران', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.create', N'ایجاد کاربر', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.edit', N'ویرایش کاربر', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.delete', N'حذف کاربر', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.view', N'مشاهده نقش‌ها', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.create', N'ایجاد نقش', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.edit', N'ویرایش نقش', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.delete', N'حذف نقش', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'settings.view', N'مشاهده تنظیمات', N'Settings');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'settings.edit', N'ویرایش تنظیمات', N'Settings');
END
GO

-- ============================================
-- 11. RolePermissions Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
        FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
        UNIQUE (RoleId, PermissionId)
    );
    DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @AdminRoleId, Id FROM Permissions;
END
GO

PRINT '✅ Database initialized successfully!';
GO
