#if UNITY_PS5
using SAS.Utilities.TagSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using Unity.SaveData.PS5;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveDataSaveSystem : ISaveSystem
{
    private IDataSerializer _serializer;

    public SaveDataSaveSystem(IContextBinder _)
    {
        _serializer = new JsonDataSerializer();
    }

    public async Task<T> Load<T>(int userId, string dirName, string fileName) where T : new()
    {
        var mountPoint = await SaveDataScopeManager.Acquire(userId, dirName, MountMode.ReadOnly);

        try
        {
            string path = Path.Combine(mountPoint.path, fileName + _serializer.FileExtension);
            if (File.Exists(path))
            {
                byte[] bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
                if (bytes != null && bytes.Length > 0)
                    return _serializer.Deserialize<T>(bytes);
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

        return new T();
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

            string writePath = Path.Combine(mountPoint.path, fileName + _serializer.FileExtension);

            byte[] bytes = _serializer.Serialize(data);
            await File.WriteAllBytesAsync(writePath, bytes).ConfigureAwait(false);

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

    public void Bind(IContextBinder binder) { }
}
#endif
