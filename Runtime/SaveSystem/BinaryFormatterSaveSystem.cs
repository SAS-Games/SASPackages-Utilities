using SAS.Utilities.TagSystem;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

public class BinaryFormatterSaveSystem : ISaveSystem
{
    private readonly string rootDir = Application.persistentDataPath;

    public BinaryFormatterSaveSystem(IContextBinder _)
    {

    }

    public async Task<T> Load<T>(int userId, string fileName) where T : new()
    {
        return await Load<T>(userId, "Saves", fileName);
    }

    public async Task<T> Load<T>(int userId, string dirName, string fileName) where T : new()
    {
        var filePath = Path.Combine(rootDir, dirName, userId.ToString(), fileName + ".dat");

        if (File.Exists(filePath))
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    var result = await Task.Run(() => (T)formatter.Deserialize(fs));
                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load file: {ex.Message}");
            }
        }

        return new T();
    }

    public async Task Save<T>(int userId, string fileName, T data)
    {
        await Save(userId, "Saves", fileName, data);
    }

    public async Task Save<T>(int userId, string dirName, string fileName, T data)
    {
        try
        {
            var directoryPath = Path.Combine(rootDir, dirName, userId.ToString());
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName + ".dat");
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                await Task.Run(() => formatter.Serialize(fs, data));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save file: {ex.Message}");
        }
    }

    void IBindable.OnInstanceCreated()
    {
    }
}
