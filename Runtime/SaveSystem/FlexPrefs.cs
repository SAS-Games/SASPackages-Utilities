using System.Collections.Generic;
using System.Threading.Tasks;

public static class FlexPrefs
{
    private static ISaveSystem _saveSystem;
    private static int _activeUserId;

    // Per-user, per-file caches
    private static readonly Dictionary<(int userId, string file), Dictionary<string, object>> _caches = new();
    private static readonly Dictionary<(int userId, string file), bool> _isDirty = new();
    private static readonly Dictionary<(int userId, string file), bool> _isSaving = new();
    private static readonly Dictionary<(int userId, string file), TaskCompletionSource<bool>> _saveTasks = new();

    private const string DefaultFile = "FlexPrefsData";
    private const string DirName = "FlexPrefsDataDir";

    public static void Initialize(ISaveSystem saveSystem, int defaultUserId = 0)
    {
        _saveSystem = saveSystem;
        _activeUserId = defaultUserId;
    }

    public static void SetActiveUser(int userId) => _activeUserId = userId;

    private static async Task EnsureCache(int userId, string fileName)
    {
        var key = (userId, fileName);

        if (_caches.ContainsKey(key))
            return;

        var data = await _saveSystem.Load<Dictionary<string, object>>(userId, DirName, fileName);
        _caches[key] = data ?? new Dictionary<string, object>();

        _isDirty[key] = false;
        _isSaving[key] = false;

        var tcs = new TaskCompletionSource<bool>();
        tcs.SetResult(true);
        _saveTasks[key] = tcs;
    }
    
    public static T Get<T>(int userId, string key, T defaultValue = default, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.ContainsKey(cacheKey))
            EnsureCache(userId, fileName).GetAwaiter().GetResult();

        var cache = _caches[cacheKey];
        return cache.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue : defaultValue;
    }

    public static void Set<T>(int userId, string key, T value, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.ContainsKey(cacheKey))
            EnsureCache(userId, fileName).GetAwaiter().GetResult();

        var cache = _caches[cacheKey];
        cache[key] = value;
        _isDirty[cacheKey] = true;
    }
    
    public static async Task Save(int userId, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.ContainsKey(cacheKey))
            await EnsureCache(userId, fileName);

        while (_isSaving[cacheKey])
            await _saveTasks[cacheKey].Task;

        if (!_isDirty[cacheKey])
            return;

        _isSaving[cacheKey] = true;
        _saveTasks[cacheKey] = new TaskCompletionSource<bool>();

        try
        {
            do
            {
                _isDirty[cacheKey] = false;
                await _saveSystem.Save(userId, DirName, fileName, _caches[cacheKey]);
            }
            while (_isDirty[cacheKey]);
        }
        finally
        {
            _isSaving[cacheKey] = false;
            _saveTasks[cacheKey].SetResult(true);
        }
    }

    public static async Task SaveAll()
    {
        var tasks = new List<Task>();

        foreach (var entry in _caches.Keys)
        {
            tasks.Add(Save(entry.userId, entry.file));
        }

        await Task.WhenAll(tasks);
    }
    
    public static T Get<T>(string key, T defaultValue = default, string fileName = DefaultFile)
        => Get(_activeUserId, key, defaultValue, fileName);

    public static void Set<T>(string key, T value, string fileName = DefaultFile)
        => Set(_activeUserId, key, value, fileName);

    public static Task Save(string fileName = DefaultFile)
        => Save(_activeUserId, fileName);
    public static bool HasKey(int userId, string key, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.ContainsKey(cacheKey))
            EnsureCache(userId, fileName).GetAwaiter().GetResult();

        return _caches[cacheKey].ContainsKey(key);
    }

    public static bool HasKey(string key, string fileName = DefaultFile)
        => HasKey(_activeUserId, key, fileName);
}
