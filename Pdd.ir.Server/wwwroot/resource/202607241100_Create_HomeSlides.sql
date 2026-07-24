-- ============================================
-- HomeSlides table - home page slider
-- ============================================

IF OBJECT_ID('HomeSlides', 'U') IS NULL
BEGIN
    CREATE TABLE HomeSlides (
        Id INT PRIMARY KEY IDENTITY(1,1),
        BadgeFa NVARCHAR(200) NOT NULL DEFAULT '',
        BadgeEn NVARCHAR(200) NOT NULL DEFAULT '',
        TitleFa NVARCHAR(500) NOT NULL DEFAULT '',
        TitleEn NVARCHAR(500) NOT NULL DEFAULT '',
        DescFa NVARCHAR(1000) NOT NULL DEFAULT '',
        DescEn NVARCHAR(1000) NOT NULL DEFAULT '',
        ImageUrl NVARCHAR(500) NOT NULL DEFAULT '',
        SortOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt BIGINT NOT NULL
    );
    
    CREATE INDEX IX_HomeSlides_SortOrder ON HomeSlides(SortOrder);
    CREATE INDEX IX_HomeSlides_IsActive ON HomeSlides(IsActive);
END
GO

-- Insert default data only if empty
IF NOT EXISTS (SELECT 1 FROM HomeSlides)
BEGIN
    DECLARE @Now BIGINT = DATEDIFF(second, '1970-01-01', GETUTCDATE());
    
    INSERT INTO HomeSlides (BadgeFa, BadgeEn, TitleFa, TitleEn, DescFa, DescEn, ImageUrl, SortOrder, IsActive, CreatedAt) VALUES
    ('راهکار هوشمند', 'Smart Solution', 'نرم‌افزار جامع مدیریت درمانی', 'Comprehensive Healthcare Software', 'طراحی سیستم‌های مدرن برای بیمارستان‌ها و مراکز درمانی با بیش از ۲۲ سال تجربه', 'Modern systems for hospitals and medical centers with over 22 years of experience', 'https://images.unsplash.com/photo-1576091160550-2173dba999ef?w=1920&q=80', 1, 1, @Now),
    ('پوشش جامع', 'Full Coverage', '۲۷ زیرسیستم یکپارچه', '27 Integrated Subsystems', 'از پذیرش بیمار تا ترخیص، از آزمایشگاه تا خزانه‌داری — همه در یک پلتفرم', 'From patient admission to discharge, lab to treasury — all in one platform', 'https://images.unsplash.com/photo-1551190822-a9ce113ac100?w=1920&q=80', 2, 1, @Now),
    ('پشتیبانی', 'Support', 'پشتیبانی ۲۴ ساعته', '24/7 Support', 'بیش از ۱۱۰ مرکز درمانی در سراسر کشور به ما اعتماد کرده‌اند', 'Over 110 medical centers across the country trust us', 'https://images.unsplash.com/photo-1579684385127-1ef15d508118?w=1920&q=80', 3, 1, @Now);
END
GO
