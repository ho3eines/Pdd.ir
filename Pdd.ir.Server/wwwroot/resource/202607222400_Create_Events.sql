-- ============================================
-- Create Events table
-- Safe to run multiple times
-- ============================================

-- Step 1: Create table if not exists
IF OBJECT_ID('Events', 'U') IS NULL
BEGIN
    CREATE TABLE Events (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(500) NOT NULL,
        TitleEn NVARCHAR(500) NOT NULL DEFAULT '',
        Description NVARCHAR(MAX) NOT NULL DEFAULT '',
        DescriptionEn NVARCHAR(MAX) NOT NULL DEFAULT '',
        ImageUrl NVARCHAR(500) NOT NULL DEFAULT '',
        Location NVARCHAR(500) NOT NULL DEFAULT '',
        EventDate BIGINT NOT NULL DEFAULT 0,
        EventEndDate BIGINT NOT NULL DEFAULT 0,
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
    PRINT 'Events table created.';
END
ELSE
BEGIN
    PRINT 'Events table already exists.';
END
GO
