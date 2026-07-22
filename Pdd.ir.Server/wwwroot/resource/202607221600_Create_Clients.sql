-- Create Clients table for Home page marquee
CREATE TABLE Clients (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    NameEn NVARCHAR(200) NOT NULL DEFAULT '',
    Icon NVARCHAR(100) NOT NULL DEFAULT 'bi-hospital',
    Color NVARCHAR(20) NOT NULL DEFAULT '#0D6EFD',
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt BIGINT NOT NULL
);

CREATE INDEX IX_Clients_SortOrder ON Clients(SortOrder);
CREATE INDEX IX_Clients_IsActive ON Clients(IsActive);
