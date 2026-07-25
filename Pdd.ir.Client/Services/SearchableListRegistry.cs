namespace Pdd.ir.Client.Services;

public static class SearchableListRegistry
{
    private static readonly Dictionary<string, ListData> _store = new();

    public static string Register(object items, Func<object, string> textSelector, object? currentValue = null)
    {
        var key = Guid.NewGuid().ToString("N")[..8];
        _store[key] = new ListData
        {
            Items = items,
            TextSelector = textSelector,
            CurrentValue = currentValue
        };
        return key;
    }

    public static ListData? Get(string key)
    {
        _store.TryGetValue(key, out var data);
        return data;
    }

    public static void Remove(string key)
    {
        _store.Remove(key);
    }

    public class ListData
    {
        public object Items { get; set; } = null!;
        public Func<object, string> TextSelector { get; set; } = null!;
        public object? CurrentValue { get; set; }
    }
}
