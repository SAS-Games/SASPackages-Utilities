using System.Threading.Tasks;
using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using UnityEngine;
using Debug = SAS.Debug;

public class SceneGroupLoader : MonoBehaviour
{
    [Inject] private ISceneLoader _sceneLoader;
    [SerializeField] private string m_SceneGroupName;
    [SerializeField] private bool m_LoadOptionalScenes = false;
    [SerializeField] private bool m_LoadOnStart = false;

    [Tooltip("Before unloading the Current Scene Group, Set an active scene.")] [SerializeField]
    private string m_SetActiveScene = "Persistent";

    [SerializeField] private GameObject m_LoadingScreen;

    private ILoadingScreen _loadingScreen;

    private async void Start()
    {
        this.InjectFieldBindings();
        if (m_LoadingScreen)
        {
            _loadingScreen = m_LoadingScreen.GetComponentInChildren<ILoadingScreen>();
            if (_loadingScreen != null)
                _loadingScreen.OnFadeOutComplete += () => GameObject.Destroy(gameObject);
        }

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
        {
            var scene = SceneUtility.GetScene(m_SetActiveScene);
            if (scene.isLoaded)
            {
                SceneUtility.MoveGameObjectToScene(gameObject, scene);
                SceneUtility.SetActiveScene(m_SetActiveScene);
            }
        }

        await _sceneLoader.LoadSceneGroup(m_SceneGroupName, !m_LoadOptionalScenes);
        Debug.Log($"Scene Group: {m_SceneGroupName} is loaded. Hide loading screen");
        _loadingScreen?.SetActive(false);
    }
}
