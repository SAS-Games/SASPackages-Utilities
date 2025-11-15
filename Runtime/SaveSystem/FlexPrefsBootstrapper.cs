using SAS.Utilities.TagSystem;
using UnityEngine;

public class FlexPrefsBootstrapper : MonoBehaviour
{
    private static FlexPrefsBootstrapper _instance;

    [Inject] private ISaveSystem _flexPrefsSaveSystem;
    [Inject] private IUserModel _userModel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateBootstrapper()
    {
        if (_instance != null)
            return;

        var existing = FindFirstObjectByType<FlexPrefsBootstrapper>();
        if (existing != null)
            return;

        var go = new GameObject("FlexPrefsBootstrapper");
        go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.NotEditable;
        DontDestroyOnLoad(go);

        go.AddComponent<FlexPrefsBootstrapper>();
    }
    
    public async void Awake()
    {
        this.InjectFieldBindings();
        if(_userModel == null || _flexPrefsSaveSystem == null)
            return;

        int userId = _userModel.GetActiveUserId();
        FlexPrefs.Initialize(_flexPrefsSaveSystem, userId);

        if (!FlexPrefs.IsUserDataLoaded(userId))
        {
            Debug.Log("[FlexPrefsBootstrapper] Preloading user data...");
            await FlexPrefs.PreloadUserAsync(userId);
        }

        Debug.Log($"[FlexPrefsBootstrapper] FlexPrefs ready for user {userId}");
    }

    private async void OnApplicationQuit()
    {
        await FlexPrefs.SaveAll();
        Debug.Log("[FlexPrefsBootstrapper] Saved all data on quit.");
    }

#if UNITY_PS5
    private async void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            await FlexPrefs.SaveAll();
            Debug.Log("[FlexPrefsBootstrapper] Saved all data on focus lost (PS5).");
        }
    }
#else
    private async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            await FlexPrefs.SaveAll();
            Debug.Log("[FlexPrefsBootstrapper] Saved all data on pause.");
        }
    }
#endif
}
