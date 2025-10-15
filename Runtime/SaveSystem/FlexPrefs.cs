using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A lightweight, async-backed PlayerPrefs-like system that loads and saves user data
/// asynchronously while keeping Get/Set synchronous for gameplay.
/// </summary>
public static class FlexPrefs
{
    private static ISaveSystem _saveSystem;
    private static int _activeUserId;

    // Cache and metadata
    private static readonly Dictionary<(int userId, string file), Dictionary<string, object>> _caches = new();
    private static readonly Dictionary<(int userId, string file), bool> _isDirty = new();
    private static readonly Dictionary<(int userId, string file), bool> _isSaving = new();

    private const string DefaultFile = "FlexPrefsData";
    private const string DirName = "FlexPrefsDataDir";

    // ------------------------------------------------------
    // Initialization
    // ------------------------------------------------------
    public static void Initialize(ISaveSystem saveSystem, int defaultUserId = 0)
    {
        _saveSystem = saveSystem;
        _activeUserId = defaultUserId;
    }

    public static void SetActiveUser(int userId) => _activeUserId = userId;

    // ------------------------------------------------------
    // Preload (Async)
    // ------------------------------------------------------
    /// <summary>
    /// Preloads all data for the specified user asynchronously.
    /// Must be called before any Get/Set for that user.
    /// </summary>
    public static async Task PreloadUserAsync(int userId, string fileName = DefaultFile)
    {
        var key = (userId, fileName);
        if (_caches.ContainsKey(key))
            return;

        try
        {
            var data = await _saveSystem.Load<Dictionary<string, object>>(userId, DirName, fileName);
            _caches[key] = data ?? new Dictionary<string, object>();
            _isDirty[key] = false;
            _isSaving[key] = false;

            Debug.Log($"[FlexPrefs] Preloaded data for user {userId} ({fileName}).");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FlexPrefs] Failed to preload data for user {userId}: {ex}");
            _caches[key] = new Dictionary<string, object>();
            _isDirty[key] = false;
            _isSaving[key] = false;
        }
    }

    public static Task PreloadActiveUserAsync(string fileName = DefaultFile)
        => PreloadUserAsync(_activeUserId, fileName);

    // ------------------------------------------------------
    // Synchronous Get / Set
    // ------------------------------------------------------
    public static T Get<T>(int userId, string key, T defaultValue = default, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.TryGetValue(cacheKey, out var cache))
        {
            Debug.LogWarning($"[FlexPrefs] Cache not loaded for user {userId}. Returning default for key '{key}'.");
            return defaultValue;
        }

        return cache.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : defaultValue;
    }

    public static void Set<T>(int userId, string key, T value, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.TryGetValue(cacheKey, out var cache))
        {
            Debug.LogWarning($"[FlexPrefs] Cache not loaded for user {userId}. Skipping Set('{key}').");
            return;
        }

        cache[key] = value;
        _isDirty[cacheKey] = true;
    }

    // Active user convenience
    public static T Get<T>(string key, T defaultValue = default, string fileName = DefaultFile)
        => Get(_activeUserId, key, defaultValue, fileName);

    public static void Set<T>(string key, T value, string fileName = DefaultFile)
        => Set(_activeUserId, key, value, fileName);

    // ------------------------------------------------------
    // Save Operations
    // ------------------------------------------------------
    public static async Task Save(int userId, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);

        if (!_caches.ContainsKey(cacheKey))
            return;

        if (!_isDirty.TryGetValue(cacheKey, out bool isDirty) || !isDirty)
            return;

        _isSaving[cacheKey] = true;

        try
        {
            _isDirty[cacheKey] = false;
            await _saveSystem.Save(userId, DirName, fileName, _caches[cacheKey]);
            Debug.Log($"[FlexPrefs] Saved data for user {userId} ({fileName}).");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FlexPrefs] Save failed for user {userId}: {ex}");
        }
        finally
        {
            _isSaving[cacheKey] = false;
        }
    }

    public static async Task SaveAll()
    {
        var tasks = new List<Task>();

        foreach (var entry in _caches.Keys)
            tasks.Add(Save(entry.userId, entry.file));

        await Task.WhenAll(tasks);
    }

    public static Task Save(string fileName = DefaultFile)
        => Save(_activeUserId, fileName);

    // ------------------------------------------------------
    // Status & Utility Checks
    // ------------------------------------------------------
    public static bool IsUserDataLoaded(int userId, string fileName = DefaultFile)
    {
        var key = (userId, fileName);
        return _caches.ContainsKey(key);
    }

    public static bool IsActiveUserDataLoaded(string fileName = DefaultFile)
    {
        return IsUserDataLoaded(_activeUserId, fileName);
    }

    /// <summary>
    /// Checks if a specific key exists for the given user.
    /// </summary>
    public static bool HasKey(int userId, string key, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);
        return _caches.TryGetValue(cacheKey, out var cache) && cache.ContainsKey(key);
    }

    public static bool HasKey(string key, string fileName = DefaultFile)
        => HasKey(_activeUserId, key, fileName);

    /// <summary>
    /// Deletes a specific key for the given user.
    /// </summary>
    public static void DeleteKey(int userId, string key, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);
        if (_caches.TryGetValue(cacheKey, out var cache) && cache.Remove(key))
            _isDirty[cacheKey] = true;
    }

    public static void DeleteKey(string key, string fileName = DefaultFile)
        => DeleteKey(_activeUserId, key, fileName);

    /// <summary>
    /// Clears all stored data for the given user and file.
    /// </summary>
    public static void Clear(int userId, string fileName = DefaultFile)
    {
        var cacheKey = (userId, fileName);
        if (_caches.ContainsKey(cacheKey))
        {
            _caches[cacheKey].Clear();
            _isDirty[cacheKey] = true;
        }
    }

    public static void Clear(string fileName = DefaultFile)
        => Clear(_activeUserId, fileName);
}
