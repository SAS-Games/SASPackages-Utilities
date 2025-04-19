using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using System.Threading.Tasks;
using UnityEngine;

public class SceneGroupLoader : MonoBehaviour
{
    [Inject] private ISceneLoader _sceneLoader;
    [SerializeField] private string m_SceneGroupName;
    [SerializeField] private bool m_LoadOptionalScenes = false;
    [SerializeField] private bool m_LoadOnStart = false;
    [Tooltip("Before unloading the Current Scene Group, Set an active scene.")]
    [SerializeField] private string m_SetActiveScene = "Persistent";
    [SerializeField] private MonoBehaviour m_LoadingScreenBehaviour;

    private ILoadingScreen _loadingScreen;

    private async void Start()
    {
        this.InjectFieldBindings();
        _loadingScreen = m_LoadingScreenBehaviour as ILoadingScreen;
        if (m_LoadOnStart)
            await LoadSceneGroup();
    }

    public void Load()
    {
        _ = LoadSceneGroup();
    }

    private async Task LoadSceneGroup()
    {
        _loadingScreen?.SetActive(true);

        if (!string.IsNullOrEmpty(m_SetActiveScene))
            SceneUtility.SetActiveScene(m_SetActiveScene);
        await _sceneLoader.LoadSceneGroup(m_SceneGroupName, !m_LoadOptionalScenes);
        _loadingScreen?.SetActive(false);
    }
}
