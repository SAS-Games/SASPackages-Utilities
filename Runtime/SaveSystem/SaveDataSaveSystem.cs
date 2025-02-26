using SAS.Utilities.TagSystem;
using System.Threading.Tasks;

public class SaveDataSaveSystem : ISaveSystem
{
    public SaveDataSaveSystem(IContextBinder _)
    {

    }

    async Task<T> ISaveSystem.Load<T>(int userId, string fileName)
    {
        return await ((ISaveSystem)this).Load<T>(userId, "Saves", fileName);
    }

    async Task<T> ISaveSystem.Load<T>(int userId, string dirName, string fileName)
    {

        //if (!SaveData.DirectoryExists(userId, dirName))
        //{
        //    Debug.Log("No save data found");
        //    return null;
        //}

        //var mountResult = SaveData.Mount(userId, dirName, SaveData.MountMode.ReadOnly);
        //if (mountResult != SaveData.Result.OK)
        //{
        //    Debug.LogError("Mount failed: " + mountResult);
        //    return null;
        //}

        //try
        //{
        //    string filePath = Path.Combine(SaveData.GetSaveDataRootPath(), fileName);

        //    if (File.Exists(filePath))
        //    {
        //        byte[] bytes = File.ReadAllBytes(filePath);
        //        string jsonData = System.Text.Encoding.UTF8.GetString(bytes);
        //        return JsonUtility.FromJson<GameData>(jsonData);
        //    }
        //    return null;
        //}
        //finally
        //{
        //    SaveData.Unmount();
        //}

        return default(T);
    }

    async Task ISaveSystem.Save<T>(int userId, string fileName, T data)
    {
        await ((ISaveSystem)this).Save(userId, "Saves", fileName, data);
    }

    async Task ISaveSystem.Save<T>(int userId, string dirName, string fileName, T data)
    {
        CreateSaveDirectory();

        // Mount the save data
        //var mountResult = SaveData.Mount(userId, dirName, SaveData.MountMode.ReadWrite);
        //if (mountResult != SaveData.Result.OK)
        //{
        //    Debug.LogError("Mount failed: " + mountResult);
        //    return;
        //}

        //try
        //{
        //    // Serialize your game data
        //    string jsonData = JsonUtility.ToJson(gameData);
        //    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        //    // Save to file
        //    File.WriteAllBytes(Path.Combine(SaveData.GetSaveDataRootPath(), fileName), bytes);

        //    Debug.Log("Game saved successfully");
        //}
        //finally
        //{
        //    // Unmount after operation
        //    SaveData.Unmount();
        //}
    }

    void IBindable.OnInstanceCreated()
    {
        //// Initialize the save data system
        //var result = SaveData.Initialize();
        //if (result != SaveData.Result.OK)
        //{
        //    Debug.LogError("Save data initialization failed: " + result);
        //}
    }

    private void CreateSaveDirectory()
    {
        // Create save data directory if it doesn't exist
        //if (!SaveData.DirectoryExists(userId, dirName))
        //{
        //    var result = SaveData.CreateDir(userId, dirName);
        //    if (result != SaveData.Result.OK)
        //    {
        //        Debug.LogError("Directory creation failed: " + result);
        //    }
        //}
    }
}
