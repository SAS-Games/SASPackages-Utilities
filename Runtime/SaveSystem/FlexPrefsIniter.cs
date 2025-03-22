using SAS.Utilities.TagSystem;
using UnityEngine;

public class FlexPrefsIniter : MonoBehaviour
{
    [Inject] ISaveSystem _flexPrefsSaveSytem;
    [Inject] IUserModel _userModel;

    async void Start()
    {
        this.InjectFieldBindings();
        await FlexPrefs.Initialize(_flexPrefsSaveSytem, _userModel.GetActiveUserId());
    }

    private void OnApplicationQuit()
    {
        _ = FlexPrefs.Save();
    }

#if UNITY_PS5
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            _ = FlexPrefs.Save();
    }
#endif
}
