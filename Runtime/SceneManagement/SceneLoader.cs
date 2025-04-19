using SAS.Utilities.TagSystem;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAS.SceneManagement
{
   public interface ILoadingScreen
    {
        void SetActive(bool active);
        Action OnFadeInComplete{get; set;}
        Action OnFadeOutComplete{get; set;}
    }

    public struct SceneGroupLoadedEvent : IEvent
    {
        public SceneGroup sceneGroup;
    }

    struct AdditiveSceneLoadedEvent : IEvent
    {
        public Scene scene;
    }

    public interface ISceneLoader : IBindable
    {
        Task LoadSceneGroup(string groupName, bool ignoreOptional = false);
        Task LoadSceneAdditively(string sceneName, IProgress<float> progress = null);
        Task UnloadScene(string name);
        event Action<float> OnProgressUpdated;
    }

    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        public event Action<float> OnProgressUpdated;
        [SerializeField] private SceneGroupsConfig m_SceneGroupsConfig;

        private readonly SceneGroupModel sceneGroupModel = new SceneGroupModel();
        private float _targetProgress;

        public async Task LoadSceneGroup(string groupName, bool ignoreOptional = false)
        {
            Debug.Log($"LoadSceneGroup GroupName: {groupName} IgnoreOptional: {ignoreOptional}", SceneGroupModel.TAG);
            int index = Array.FindIndex(m_SceneGroupsConfig.SceneGroups, sceneGroup => sceneGroup.Name == groupName);
            if (index == -1)
            {
                Debug.LogWarning($"Scene Group with Name: {groupName} not found in configuration list");
                return;
            }

            await LoadSceneGroup(index, ignoreOptional);
        }

        public async Task LoadSceneGroup(int index, bool ignoreOptional = false)
        {
            _targetProgress = 1f;

            if (index < 0 || index >= m_SceneGroupsConfig.SceneGroups.Length)
            {
                Debug.LogError("Invalid scene group index: " + index);
                return;
            }

            LoadingProgress progress = new LoadingProgress();
            progress.Progressed += target =>
            {
                _targetProgress = Mathf.Max(target, _targetProgress);
                OnProgressUpdated?.Invoke(_targetProgress);
            };
            OnProgressUpdated?.Invoke(0f);
            await sceneGroupModel.LoadScenes(m_SceneGroupsConfig.SceneGroups[index], progress, false, ignoreOptional);
            await Task.Delay(1000);
        }

        public async Task LoadSceneAdditively(string sceneName, IProgress<float> progress = null)
        {
            await sceneGroupModel.LoadSceneAdditively(sceneName, progress);
        }

        public async Task UnloadScenes()
        {
            await sceneGroupModel.UnloadScenes();
        }

        public async Task UnloadScene(string sceneName)
        {
            await sceneGroupModel.UnloadScene(sceneName);
        }

        public void OnInstanceCreated() { }
    }

    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;

        const float ratio = 1f;

        public void Report(float value)
        {
            Progressed?.Invoke(value / ratio);
        }
    }
}
