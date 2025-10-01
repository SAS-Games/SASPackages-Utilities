#if UNITY_PS5
public static class SaveDataScopeManager
{
    private class MountRef
    {
        public MountPoint MountPoint;
        public int RefCount;
    }

    private static readonly Dictionary<string, MountRef> mounts = new();

    private static string Key(int userId, string dirName) => $"{userId}:{dirName}";

    public static async Task<MountPoint> Acquire(int userId, string dirName, MountMode mode)
    {
        var key = Key(userId, dirName);

        if (mounts.TryGetValue(key, out var mountRef))
        {
            mountRef.RefCount++;
            return mountRef.MountPoint;
        }

        var mountParams = new MountParams
        {
            userID = userId,
            blocks = Constants.MinBlocks,
            mountMode = mode,
            dirName = dirName
        };

        var mount = await SaveData.Mount(mountParams);
        Assert.IsTrue(mount.ReturnCode.isOk, $"Error during mount: {mount.ReturnCode}");

        mounts[key] = new MountRef { MountPoint = mount.OperationResult.mountPoint, RefCount = 1 };
        return mounts[key].MountPoint;
    }

    public static async Task Release(int userId, string dirName)
    {
        var key = Key(userId, dirName);

        if (!mounts.TryGetValue(key, out var mountRef))
            return;

        mountRef.RefCount--;

        if (mountRef.RefCount <= 0)
        {
            var unmount = await SaveData.Unmount(UnmountMode.Default, mountRef.MountPoint);
            Assert.IsTrue(unmount.ReturnCode.isOk, $"Error during unmount: {unmount}");
            mounts.Remove(key);
        }
    }
}
#endif
