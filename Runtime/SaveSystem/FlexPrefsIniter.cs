using SAS.Utilities.TagSystem;
using UnityEngine;
using Debug = SAS.Debug;

public class FlexPrefsIniter : MonoBehaviour
{
    [Inject(Tag.FlexPrefs)] ISaveSystem _flexPrefsSaveSytem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        this.InjectFieldBindings();
        await FlexPrefs.Initialize(_flexPrefsSaveSytem);
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
