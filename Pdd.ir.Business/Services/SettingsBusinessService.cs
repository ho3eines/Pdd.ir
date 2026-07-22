using Pdd.ir.Data;

namespace Pdd.ir.Business.Services
{
    public class SettingsBusinessService
    {
        private readonly IDbService _db;

        public SettingsBusinessService(IDbService db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var settings = await _db.QueryAsync<SettingDto>("SELECT [Key], [Value] FROM SiteSettings");
            return settings.ToDictionary(s => s.Key, s => s.Value);
        }

        public async Task<string?> GetAsync(string key)
        {
            var setting = await _db.QueryFirstOrDefaultAsync<SettingDto>(
                "SELECT [Key], [Value] FROM SiteSettings WHERE [Key] = @Key",
                new { Key = key });
            return setting?.Value;
        }

        public async Task SetAsync(string key, string value)
        {
            var exists = await _db.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM SiteSettings WHERE [Key] = @Key",
                new { Key = key });

            if (exists > 0)
            {
                await _db.ExecuteAsync(
                    "UPDATE SiteSettings SET [Value] = @Value WHERE [Key] = @Key",
                    new { Key = key, Value = value });
            }
            else
            {
                await _db.ExecuteAsync(
                    "INSERT INTO SiteSettings ([Key], [Value]) VALUES (@Key, @Value)",
                    new { Key = key, Value = value });
            }
        }

        public async Task SetManyAsync(Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                await SetAsync(kvp.Key, kvp.Value);
            }
        }
    }

    public class SettingDto
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
