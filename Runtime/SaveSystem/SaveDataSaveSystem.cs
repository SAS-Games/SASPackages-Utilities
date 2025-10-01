#if UNITY_PS5
using Newtonsoft.Json;
using SAS.Utilities.TagSystem;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.SaveData.PS5;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveDataSaveSystem : ISaveSystem
{
    public SaveDataSaveSystem(IContextBinder _) { }

    public async Task<T> Load<T>(int userId, string dirName, string fileName)
    {
        var mountPoint = await SaveDataScopeManager.Acquire(userId, dirName, MountMode.ReadOnly);

        try
        {
            string path = Path.Combine(mountPoint.path, fileName);
            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path, Encoding.UTF8).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    return JsonConvert.DeserializeObject<T>(json, JsonSettings.Settings);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed for {dirName}/{fileName}: {ex}");
        }
        finally
        {
            await SaveDataScopeManager.Release(userId, dirName);
        }

        return default;
    }

    public async Task Save<T>(int userId, string dirName, string fileName, T data)
    {
        var mountPoint = await SaveDataScopeManager.Acquire(userId, dirName, MountMode.ReadWrite | MountMode.Create2);

        try
        {
            TransactionResourceId.CreateTransactionResource(out var id);

            var prepare = await SaveData.Prepare(mountPoint, new PrepareParam
            {
                mode = PrepareMode.Default,
                resource = id
            });
            Assert.IsTrue(prepare.ReturnCode.isOk, $"Error during Prepare: {prepare.ReturnCode}");

            string writePath = Path.Combine(mountPoint.path, fileName);
            string json = JsonConvert.SerializeObject(data, JsonSettings.Settings);
            await File.WriteAllTextAsync(writePath, json, Encoding.UTF8).ConfigureAwait(false);

            var commit = await SaveData.Commit(new CommitParam
            {
                commitMode = CommitMode.Default,
                resource = id
            });
            Assert.IsTrue(commit.ReturnCode.isOk, $"Error during Commit: {commit.ReturnCode}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed for {dirName}/{fileName}: {ex}");
        }
        finally
        {
            await SaveDataScopeManager.Release(userId, dirName);
        }
    }
}
#endif
