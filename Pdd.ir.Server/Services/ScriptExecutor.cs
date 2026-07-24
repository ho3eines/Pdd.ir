using Pdd.ir.Data;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Pdd.ir.Server.Services
{
    public class ScriptExecutor
    {
        private readonly IDbService _db;
        private readonly ILogger<ScriptExecutor> _logger;

        private string ConnStr => Environment.GetEnvironmentVariable("ConnectionString")
            ?? "workstation id=support;password=123456;packet size=4096;user id=sa;data source=.;persist security info=false;initial catalog=pdd;Encrypt=False";

        public ScriptExecutor(IDbService db, ILogger<ScriptExecutor> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ExecutePendingScriptsAsync(string scriptsPath)
        {
            try
            {
                EnsureResourceExecuteTable();

                if (!Directory.Exists(scriptsPath))
                {
                    _logger.LogWarning("Scripts path not found: {Path}", scriptsPath);
                    return;
                }

                var executedFiles = (await _db.QueryAsync<string>(
                    "SELECT FileName FROM ResurceExecute WHERE Status = 'OK'")).ToHashSet();

                var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql")
                    .OrderBy(f => Path.GetFileName(f))
                    .ToList();

                foreach (var file in sqlFiles)
                {
                    var fileName = Path.GetFileName(file);

                    if (executedFiles.Contains(fileName))
                        continue;

                    _logger.LogInformation("Executing script: {File}", fileName);

                    var sql = await File.ReadAllTextAsync(file);
                    var statements = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    bool hasError = false;
                    string? errorMsg = null;

                    foreach (var statement in statements)
                    {
                        var trimmed = statement.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;

                        try
                        {
                            using var connection = new SqlConnection(ConnStr);
                            await connection.OpenAsync();
                            using var cmd = connection.CreateCommand();
                            cmd.CommandText = trimmed;
                            cmd.CommandTimeout = 120;
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            errorMsg = ex.Message;
                            _logger.LogWarning("Statement failed in {File}: {Error}", fileName, ex.Message);
                        }
                    }

                    // Record execution result
                    await _db.ExecuteAsync(
                        "INSERT INTO ResurceExecute (FileName, ExecutedAt, Status, ErrorMessage) VALUES (@FileName, @ExecutedAt, @Status, @ErrorMessage)",
                        new
                        {
                            FileName = fileName,
                            ExecutedAt = DateTime.UtcNow,
                            Status = hasError ? "ERROR" : "OK",
                            ErrorMessage = errorMsg ?? ""
                        });

                    if (hasError)
                        _logger.LogWarning("Script completed with errors: {File}", fileName);
                    else
                        _logger.LogInformation("Script executed successfully: {File}", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scripts");
            }
        }

        private void EnsureResourceExecuteTable()
        {
            try
            {
                var sql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ResurceExecute')
                    BEGIN
                        CREATE TABLE ResurceExecute (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            FileName NVARCHAR(256) NOT NULL,
                            ExecutedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            Status NVARCHAR(10) NOT NULL DEFAULT 'OK',
                            ErrorMessage NVARCHAR(MAX) NULL
                        );
                    END
                    ELSE
                    BEGIN
                        -- Add columns if missing (for existing tables)
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ResurceExecute') AND name = 'Status')
                            ALTER TABLE ResurceExecute ADD Status NVARCHAR(10) NOT NULL DEFAULT 'OK';
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ResurceExecute') AND name = 'ErrorMessage')
                            ALTER TABLE ResurceExecute ADD ErrorMessage NVARCHAR(MAX) NULL;
                    END";

                using var connection = new SqlConnection(ConnStr);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring ResurceExecute table");
            }
        }
    }
}
