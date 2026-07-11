using Pdd.ir.Data;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Pdd.ir.Server.Services
{
    public class ScriptExecutor
    {
        private readonly IDbService _db;
        private readonly ILogger<ScriptExecutor> _logger;

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
                    "SELECT FileName FROM ResurceExecute")).ToHashSet();

                var sqlFiles = Directory.GetFiles(scriptsPath, "*.sql")
                    .OrderBy(f => Path.GetFileName(f))
                    .ToList();

                var connStr = Environment.GetEnvironmentVariable("ConnectionString")
                    ?? "workstation id=support;password=123456;packet size=4096;user id=sa;data source=.;persist security info=false;initial catalog=pdd;Encrypt=False";

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

                    foreach (var statement in statements)
                    {
                        var trimmed = statement.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;

                        try
                        {
                            using var connection = new SqlConnection(connStr);
                            await connection.OpenAsync();
                            using var cmd = connection.CreateCommand();
                            cmd.CommandText = trimmed;
                            cmd.CommandTimeout = 120;
                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Statement failed: {Error}\nSQL: {Sql}",
                                ex.Message, trimmed.Length > 200 ? trimmed[..200] + "..." : trimmed);
                        }
                    }

                    await _db.ExecuteAsync(
                        "INSERT INTO ResurceExecute (FileName, ExecutedAt) VALUES (@FileName, @ExecutedAt)",
                        new { FileName = fileName, ExecutedAt = DateTime.UtcNow });

                    _logger.LogInformation("Script executed: {File}", fileName);
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
                            ExecutedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                        );
                    END";

                var connStr = Environment.GetEnvironmentVariable("ConnectionString")
                    ?? "workstation id=support;password=123456;packet size=4096;user id=sa;data source=.;persist security info=false;initial catalog=pdd;Encrypt=False";

                using var connection = new SqlConnection(connStr);
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
