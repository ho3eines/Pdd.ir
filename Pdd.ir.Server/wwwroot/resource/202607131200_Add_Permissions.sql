-- ============================================
-- Permissions & RolePermissions Tables
-- Migration: 2026-07-13
-- ============================================

-- Permissions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE Permissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Label NVARCHAR(200) NOT NULL,
        Category NVARCHAR(100) NOT NULL DEFAULT 'General'
    );

    -- Product permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.view', N'مشاهده محصولات', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.create', N'ایجاد محصول', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.edit', N'ویرایش محصول', N'Products');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'product.delete', N'حذف محصول', N'Products');

    -- Blog permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.view', N'مشاهده بلاگ', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.create', N'ایجاد مقاله', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.edit', N'ویرایش مقاله', N'Blog');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'blog.delete', N'حذف مقاله', N'Blog');

    -- Portfolio permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.view', N'مشاهده نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.create', N'ایجاد نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.edit', N'ویرایش نمونه کار', N'Portfolio');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'portfolio.delete', N'حذف نمونه کار', N'Portfolio');

    -- Contact permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'contact.view', N'مشاهده پیام‌ها', N'Contact');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'contact.delete', N'حذف پیام', N'Contact');

    -- Page permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.view', N'مشاهده صفحات', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.create', N'ایجاد صفحه', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.edit', N'ویرایش صفحه', N'Pages');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'page.delete', N'حذف صفحه', N'Pages');

    -- User management permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.view', N'مشاهده کاربران', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.create', N'ایجاد کاربر', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.edit', N'ویرایش کاربر', N'Users');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'user.delete', N'حذف کاربر', N'Users');

    -- Role management permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.view', N'مشاهده نقش‌ها', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.create', N'ایجاد نقش', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.edit', N'ویرایش نقش', N'Roles');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'role.delete', N'حذف نقش', N'Roles');

    -- Settings permissions
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'settings.view', N'مشاهده تنظیمات', N'Settings');
    INSERT INTO Permissions (Name, Label, Category) VALUES (N'settings.edit', N'ویرایش تنظیمات', N'Settings');
END
GO

-- RolePermissions Table (junction)
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

    -- Admin gets all permissions
    DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @AdminRoleId, Id FROM Permissions;
END
GO
