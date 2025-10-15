using SAS.Utilities.TagSystem;
using UnityEngine;

public class FlexPrefsIniter : MonoBehaviour
{
    [Inject] private ISaveSystem _flexPrefsSaveSystem;
    [Inject] private IUserModel _userModel;

    private async void Start()
    {
        this.InjectFieldBindings();

        int userId = _userModel.GetActiveUserId();
        FlexPrefs.Initialize(_flexPrefsSaveSystem, userId);

        if (!FlexPrefs.IsUserDataLoaded(userId))
        {
            Debug.Log("[FlexPrefsIniter] Preloading user data...");
            await FlexPrefs.PreloadUserAsync(userId);
        }

        Debug.Log($"[FlexPrefsIniter] FlexPrefs ready for user {userId}");
    }

    private async void OnApplicationQuit()
    {
        await FlexPrefs.SaveAll();
        Debug.Log("[FlexPrefsIniter] Saved all data on quit.");
    }

#if UNITY_PS5
    private async void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            await FlexPrefs.SaveAll();
            Debug.Log("[FlexPrefsIniter] Saved all data on focus lost (PS5).");
        }
    }
#else
    private async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            await FlexPrefs.SaveAll();
            Debug.Log("[FlexPrefsIniter] Saved all data on pause.");
        }
    }
#endif
}
