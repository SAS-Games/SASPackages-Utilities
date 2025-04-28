using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace SAS.SceneManagement
{
    public class SceneGroupModel
    {
        public const string TAG = "SceneManagement";
        public const string BootstrapperScene = "Bootstrapper";
        public const string PersistentScene = "Persistent";
        public event Action<string> OnSceneLoaded = delegate { };
        public event Action<string> OnSceneUnloaded = delegate { };

        private readonly AsyncOperationHandleGroup _handleGroup = new AsyncOperationHandleGroup(10);

        private SceneGroup _activeSceneGroup;

        internal async Task LoadScenes(SceneGroup group, IProgress<float> progress = null, bool reloadDupScenes = false,
            bool ignoreOptional = false)
        {
            _activeSceneGroup = group;
            var loadedScenes = new HashSet<string>();

            await UnloadScenes();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            var scenesToLoad = group.Scenes.Where(scene =>
                (!reloadDupScenes && loadedScenes.Contains(scene.Name)) == false &&
                (!ignoreOptional || !scene.IsOptinal)).ToList();

            if (scenesToLoad.Count == 0)
            {
                Debug.LogWarning("No scenes to load in the group.");
                return;
            }

            var operationGroup = new AsyncOperationGroup(scenesToLoad.Count);

            foreach (var sceneData in scenesToLoad)
            {
                AsyncOperation operation = null;

                if (sceneData.Reference.State == SceneReferenceState.Regular)
                {
                    operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);
                }
                else if (sceneData.Reference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                    _handleGroup.Handles.Add(sceneHandle);
                }

                OnSceneLoaded.Invoke(sceneData.Name);
            }

            while (!operationGroup.IsDone || !_handleGroup.IsDone)
            {
                Debug.Log($"current progress: {(operationGroup.Progress + _handleGroup.Progress) / 2}", TAG);
                progress?.Report((operationGroup.Progress + _handleGroup.Progress) / 2);
                await Task.Delay(100);
                Debug.Log($"Scene Group: {_activeSceneGroup.Name} is loaded");
            }

            SetActiveScene(group);
            EventBus<SceneGroupLoadedEvent>.Raise(new SceneGroupLoadedEvent { sceneGroup = group });
        }

        internal async Task LoadSceneAdditively(string sceneName, IProgress<float> progress = null)
        {
            if (_activeSceneGroup == null)
            {
                Debug.LogWarning("No active scene group set. Cannot load scene.");
                return;
            }

            var sceneData = _activeSceneGroup.Scenes.FirstOrDefault(scene => scene.Name == sceneName);
            if (sceneData == null)
            {
                Debug.LogError($"Scene {sceneName} not found in the active scene group.");
                return;
            }

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                Debug.LogWarning($"Scene {sceneName} is already loaded.");
                return;
            }

            AsyncOperation operation = null;
            AsyncOperationHandle<SceneInstance> sceneHandle = default;
            if (sceneData.Reference.State == SceneReferenceState.Regular)
            {
                operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                while (!operation.isDone)
                {
                    progress?.Report((operation.progress + sceneHandle.PercentComplete) / 2);
                    await Task.Delay(100);
                }
            }
            else if (sceneData.Reference.State == SceneReferenceState.Addressable)
            {
                sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                _handleGroup.Handles.Add(sceneHandle);
                while (!sceneHandle.IsDone)
                {
                    progress?.Report((operation.progress + sceneHandle.PercentComplete) / 2);
                    await Task.Delay(100);
                }
            }

            EventBus<AdditiveSceneLoadedEvent>.Raise(new AdditiveSceneLoadedEvent
                { scene = SceneManager.GetSceneByName(sceneName) });
        }

        internal async Task UnloadScenes()
        {
            var scenesToUnload = new List<string>();
            var activeSceneName = SceneManager.GetActiveScene().name;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || scene.name == activeSceneName || scene.name == BootstrapperScene ||
                    scene.name == PersistentScene)
                    continue;

                scenesToUnload.Add(scene.name);
            }

            var operationGroup = new AsyncOperationGroup(scenesToUnload.Count);

            foreach (var sceneName in scenesToUnload)
            {
                var operation = SceneManager.UnloadSceneAsync(sceneName);
                if (operation != null)
                    operationGroup.Operations.Add(operation);
                OnSceneUnloaded.Invoke(sceneName);
            }

            foreach (var handle in _handleGroup.Handles.ToList())
            {
                if (handle.IsValid() && scenesToUnload.Contains(handle.Result.Scene.name))
                {
                    Addressables.UnloadSceneAsync(handle);
                }
            }

            _handleGroup.Handles.Clear(); //todo: should we clear all 

            while (!operationGroup.IsDone)
            {
                await Task.Delay(100);
            }

            await Resources.UnloadUnusedAssets();
        }

        internal async Task UnloadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty. Cannot unload.");
                return;
            }

            if (!IsSceneLoaded(sceneName))
            {
                Debug.LogWarning($"Scene {sceneName} is not loaded.");
                return;
            }

            await UnloadSceneInternal(sceneName);
        }

        public async Task ReloadScene(string sceneName, IProgress<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty. Cannot reload.");
                return;
            }

            // Check if the scene exists in the active scene group
            var sceneData = _activeSceneGroup?.Scenes.FirstOrDefault(scene => scene.Name == sceneName);
            if (sceneData == null)
            {
                Debug.LogWarning($"Scene {sceneName} not found in the active scene group.");
                return;
            }

            // Ensure the scene is loaded
            if (!IsSceneLoaded(sceneName))
            {
                Debug.LogWarning($"Scene {sceneName} is not currently loaded. Loading it instead.");
                await LoadSceneAsync(sceneData, progress);
                return;
            }

            Debug.Log($"Reloading scene: {sceneName}");

            // Unload the scene
            await UnloadSceneInternal(sceneName);

            // Reload the scene
            await LoadSceneAsync(sceneData, progress);
        }

        private async Task LoadSceneAsync(SceneData sceneData, IProgress<float> progress)
        {
            AsyncOperation operation = null;
            if (sceneData.Reference.State == SceneReferenceState.Regular)
                operation = SceneManager.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);

            else if (sceneData.Reference.State == SceneReferenceState.Addressable)
            {
                var sceneHandle = Addressables.LoadSceneAsync(sceneData.Reference.Path, LoadSceneMode.Additive);
                _handleGroup.Handles.Add(sceneHandle);
            }

            while (operation != null && !operation.isDone)
            {
                progress?.Report(operation.progress);
                await Task.Delay(100);
            }

            OnSceneLoaded.Invoke(sceneData.Name);
        }

        private void SetActiveScene(SceneGroup group)
        {
            var activeSceneName = group.FindSceneNameByType(SceneType.ActiveScene);
            if (string.IsNullOrEmpty(activeSceneName)) return;
            SceneUtility.SetActiveScene(activeSceneName);
        }

        private bool IsSceneLoaded(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).isLoaded ||
                   _handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName);
        }

        public async Task ActivateScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty. Cannot activate.");
                return;
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
            {
                Debug.LogError($"Scene {sceneName} is not valid or not loaded.");
                return;
            }

            if (scene.isLoaded && scene.isDirty)
            {
                Debug.LogWarning($"Scene {sceneName} is already active or dirty.");
                return;
            }

            if (scene.IsValid() && SceneManager.GetActiveScene() != scene)
            {
                // Activate the scene
                SceneManager.SetActiveScene(scene);
                Debug.Log($"Activated scene: {sceneName}");
            }
            else
                Debug.LogWarning($"Scene {sceneName} is not loaded or is already active.");

            // If the scene was loaded using Addressables, ensure activation
            var handle = _handleGroup.Handles.FirstOrDefault(h => h.IsValid() && h.Result.Scene.name == sceneName);
            if (handle.IsValid() && !handle.Result.Scene.isDirty)
            {
                var activationOperation = handle.Result.ActivateAsync();
                while (!activationOperation.isDone)
                    await Task.Delay(100);
                await Task.Delay(100);

                Debug.Log($"Scene {sceneName} activated via Addressables.");
            }
        }

        private async Task UnloadSceneInternal(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            // Skip unloading if it's the active scene or bootstrapper
            if (scene.name == SceneManager.GetActiveScene().name || scene.name == "Bootstrapper")
            {
                Debug.LogWarning($"Cannot unload the active scene or bootstrapper: {sceneName}");
                return;
            }

            var handle = _handleGroup.Handles.FirstOrDefault(h => h.IsValid() && h.Result.Scene.name == sceneName);

            if (scene.isLoaded)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                while (!operation.isDone)
                {
                    await Task.Delay(100);
                }
            }

            if (handle.IsValid())
            {
                await Addressables.UnloadSceneAsync(handle).Task;
                _handleGroup.Handles.Remove(handle);
            }

            OnSceneUnloaded.Invoke(sceneName);
        }
    }

    public readonly struct AsyncOperationGroup
    {
        public readonly List<AsyncOperation> Operations;

        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);
        public bool IsDone => Operations.All(o => o.isDone);

        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }

    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}