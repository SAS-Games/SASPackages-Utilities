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
        Debug.Log("FlexPrefs has been initialized");
        if(FlexPrefs.HasKey("Name"))
        {
            Debug.Log(FlexPrefs.Get<string>("Name", "Abhishek"));
        }
        else
            FlexPrefs.Set<string>("Name", "Abhishek");

    }

    private void OnApplicationQuit()
    {
        FlexPrefs.Save();
    }
}
