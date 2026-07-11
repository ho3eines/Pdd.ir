-- Blog Posts Table
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
    VALUES (N'معرفی سیستم HIS', N'his-introduction', N'آشنایی با سیستم اطلاعات بیمارستانی Pdd.ir', N'سیستم HIS یک راهکار جامع برای مدیریت بیمارستان‌ها و مراکز درمانی است که شامل ماژول‌های پذیرش، بخش‌ها، آزمایشگاه، داروخانه و صندوق می‌باشد.', N'Admin', 1);

    INSERT INTO BlogPosts (Title, Slug, Summary, Content, Author, IsPublished)
    VALUES (N'مزایای نرم‌افزار مالی', N'financial-software-benefits', N'چرا نرم‌افزار مالی Pdd.ir بهترین انتخاب است؟', N'نرم‌افزار مالی Pdd.ir با ارائه قابلیت‌های پیشرفته شامل دفتر کل، حسابهای دریافتنی و پرداختنی، گزارش‌های مالی و مدیریت جریان نقدینگی، بهترین انتخاب برای سازمان‌هاست.', N'Admin', 1);
END
GO
