using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class FlexPrefs
{
    private static ISaveSystem _saveSystem;
    private static int _userId;
    private static Dictionary<string, object> _cache = new Dictionary<string, object>();
    private static readonly string fileName = "FlexPrefsData";
    private static volatile bool _isSaving = false;
    private static bool _isDirty = true;
    private static TaskCompletionSource<bool> _saveTaskCompletion = new TaskCompletionSource<bool>();

    /// <summary>
    /// Initializes FlexPrefs with the specified save system and user ID.
    /// This must be called before using any other methods in FlexPrefs.
    /// </summary>
    /// <param name="saveSystem">The save system to use for data persistence.</param>
    /// <param name="userId">The user identifier for saving/loading data. Defaults to 0.</param>
    public static async Task Initialize(ISaveSystem saveSystem, int userId = 0)
    {
        _saveSystem = saveSystem;
        _userId = userId;
        _cache = await LoadData();
    }

    /// <summary>
    /// Loads the saved data into the cache asynchronously.
    /// If no data is found, a new empty dictionary is returned.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.
    /// The result contains the loaded data or an empty dictionary if no data is found.</returns>
    private static async Task<Dictionary<string, object>> LoadData()
    {
        var data = await _saveSystem.Load<Dictionary<string, object>>(_userId, fileName);
        return data ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// If the key does not exist, the default value is returned.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="defaultValue">The default value to return if the key does not exist. Optional.</param>
    /// <returns>The value of type T if the key exists, otherwise the default value.</returns>
    public static T Get<T>(string key, T defaultValue = default)
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");
        return _cache.TryGetValue(key, out var value) && value is T typedValue ? typedValue : defaultValue;
    }

    /// <summary>
    /// Sets the value for the specified key.
    /// If the key already exists, the value is updated.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store.</param>
    public static void Set<T>(string key, T value)
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");
        _cache[key] = value;
        _isDirty = true;
    }

    /// <summary>
    /// Saves the current state of the cache to the persistent storage asynchronously.
    /// Handles concurrent save requests gracefully by queuing them.
    /// </summary>
    public static async void Save()
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");

        if (_isSaving)
        {
            await _saveTaskCompletion.Task;
        }

        if (!_isDirty) return; //todo: need to revisit this

        _isSaving = true;
        _saveTaskCompletion = new TaskCompletionSource<bool>();

        try
        {
            await _saveSystem.Save(_userId, fileName, _cache);
            _isDirty = false;
        }
        finally
        {
            _isSaving = false;
            _saveTaskCompletion.SetResult(true);
        }
    }

    /// <summary>
    /// Checks if a specific key exists in the cache.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <returns>True if the key exists, otherwise false.</returns>
    public static bool HasKey(string key)
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");
        return _cache.ContainsKey(key);
    }

    /// <summary>
    /// Clears the value associated with the specified key.
    /// If the key exists, it is removed from the cache.
    /// </summary>
    /// <param name="key">The key to remove from the cache.</param>
    public static void ClearKey(string key)
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");

        if (_cache.Remove(key))
        {
            _isDirty = true;
        }
    }

    /// <summary>
    /// Clears all keys and values from the cache.
    /// This also marks the state as dirty, which triggers saving the cleared state.
    /// </summary>
    public static void Clear()
    {
        Debug.Assert(_saveSystem != null, "FlexPrefs not initialized. Call FlexPrefs.Initialize() first.");
        _cache.Clear();
        _isDirty = true;
    }
}
