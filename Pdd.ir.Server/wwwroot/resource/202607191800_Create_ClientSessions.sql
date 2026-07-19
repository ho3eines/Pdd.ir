-- Create ClientSessions table for session-based authentication
-- Handshake: client sends encrypted {clientId, timestamp} → server validates → returns encrypted sessionToken

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClientSessions' AND xtype='U')
BEGIN
    CREATE TABLE ClientSessions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClientId NVARCHAR(64) NOT NULL,
        SessionToken NVARCHAR(256) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_ClientSessions_ClientId ON ClientSessions(ClientId);
    CREATE INDEX IX_ClientSessions_SessionToken ON ClientSessions(SessionToken);
END
