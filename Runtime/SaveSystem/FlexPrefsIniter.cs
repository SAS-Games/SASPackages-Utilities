using SAS.Utilities.TagSystem;
using UnityEngine;

public class FlexPrefsIniter : MonoBehaviour
{
    [Inject(Tag.FlexPrefs)] ISaveSystem _flexPrefsSaveSytem;
    [Inject] IUserModel _userModel;

    async void Start()
    {
        this.InjectFieldBindings();
        await FlexPrefs.Initialize(_flexPrefsSaveSytem,_userModel.GetActiveUserId());
    }

    private void OnApplicationQuit()
    {
        FlexPrefs.Save();
    }

#if UNITY_PS5
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
           FlexPrefs.Save();
    }
#endif
}
