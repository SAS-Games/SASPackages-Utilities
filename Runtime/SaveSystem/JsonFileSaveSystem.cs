using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SAS.Utilities.TagSystem;
using UnityEngine;

public class JsonFileSaveSystem : ISaveSystem
{
    private readonly string rootDir = Application.persistentDataPath;

    public JsonFileSaveSystem(IContextBinder _) { }

    async Task<T> ISaveSystem.Load<T>(int userId, string dirName, string fileName)
    {
        var filePath = Path.Combine(rootDir, dirName, userId.ToString(), fileName + ".json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found: {filePath}");
            return default;
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonConvert.DeserializeObject<T>(json, JsonSettings.Settings);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load/deserialize file {filePath}: {ex}");
        }

        return default;
    }

    async Task ISaveSystem.Save<T>(int userId, string dirName, string fileName, T data)
    {
        try
        {
            var directoryPath = Path.Combine(rootDir, dirName, userId.ToString());
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, fileName + ".json");
            string json = JsonConvert.SerializeObject(data, JsonSettings.Settings);

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save file for {dirName}/{fileName}: {ex}");
        }
    }
}