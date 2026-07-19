-- AuthSessions table for session-based authentication
-- Stores TOKEN HASH only (never raw token)

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuthSessions' AND xtype='U')
BEGIN
    CREATE TABLE AuthSessions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClientId NVARCHAR(64) NOT NULL,
        TokenHash NVARCHAR(256) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_AuthSessions_ClientId ON AuthSessions(ClientId);
    CREATE INDEX IX_AuthSessions_TokenHash ON AuthSessions(TokenHash);
    CREATE INDEX IX_AuthSessions_Active ON AuthSessions(ClientId, IsActive) WHERE IsActive = 1;
END
