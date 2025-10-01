using SAS.Utilities.TagSystem;
using UnityEngine;

public class FlexPrefsIniter : MonoBehaviour
{
    [Inject] ISaveSystem _flexPrefsSaveSystem;
    [Inject] IUserModel _userModel;

    void Start()
    {
        this.InjectFieldBindings();
        FlexPrefs.Initialize(_flexPrefsSaveSystem, _userModel.GetActiveUserId());
    }

    private void OnApplicationQuit()
    {
        // Block until saves are finished (safe for all platforms)
        FlexPrefs.SaveAll().GetAwaiter().GetResult();
    }

#if UNITY_PS5
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            FlexPrefs.SaveAll().GetAwaiter().GetResult();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            FlexPrefs.SaveAll().GetAwaiter().GetResult();
    }
#else
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            FlexPrefs.SaveAll().GetAwaiter().GetResult();
    }
#endif
}