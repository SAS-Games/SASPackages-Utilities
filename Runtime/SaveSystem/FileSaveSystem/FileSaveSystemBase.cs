using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public abstract class FileSaveSystemBase : ISaveSystem
{
    private readonly string _rootDir = Application.persistentDataPath;
    protected IDataSerializer _serializer;

    public async Task<T> Load<T>(int userId, string dirName, string fileName) where T : new()
    {
        string filePath = GetFilePath(userId, dirName, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveSystem] File not found: {filePath}");
            return new T();
        }

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            if (bytes?.Length > 0)
                return _serializer.Deserialize<T>(bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed for {filePath}: {ex}");
        }

        return new T();
    }

    public async Task Save<T>(int userId, string dirName, string fileName, T data)
    {
        string filePath = GetFilePath(userId, dirName, fileName);
        EnsureDirectoryExists(filePath);

        try
        {
            byte[] bytes = _serializer.Serialize(data);
            await File.WriteAllBytesAsync(filePath, bytes).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed for {filePath}: {ex}");
        }
    }

    private string GetFilePath(int userId, string dirName, string fileName)
    {
        return Path.Combine(_rootDir, dirName, userId.ToString(), fileName + _serializer.FileExtension);
    }

    private void EnsureDirectoryExists(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }
}
