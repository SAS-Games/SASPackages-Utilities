using Eflatun.SceneReference;
using SAS.SceneManagement;
using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAS.Teleport
{
    public class Teleporter : MonoBehaviour
    {
        [Inject] private ISceneLoader _sceneLoader;
        [SerializeField] private bool m_UnloadCurrentScene;
        [SerializeField] private SceneReference m_DestinationScene;
        [SerializeField] private string m_DestSpawnName;

        private void Start()
        {
            this.InjectFieldBindings();
        }

        public void OnEnter(ITeleportable teleportable)
        {
            if (!teleportable.CanTeleport)
                return;
            teleportable.CanTeleport = false;

            if (gameObject.scene.name == m_DestinationScene.Name)
                Teleport(teleportable, gameObject.scene);
            else
                TeleportToNewScene(m_DestinationScene.Name, teleportable, m_UnloadCurrentScene);
        }

        private async void TeleportToNewScene(string sceneName, ITeleportable teleportable, bool unloadCurrentScene = false)
        {
            Scene currentScene = gameObject.scene;
            await _sceneLoader.LoadSceneAdditively(m_DestinationScene.Name);
            Scene nextScene = SceneManager.GetSceneByName(sceneName);
            Teleport(teleportable, nextScene);
            if (unloadCurrentScene)
                await _sceneLoader.UnloadScene(currentScene.name);
        }

        private void Teleport(ITeleportable teleportable, Scene scene)
        {
            SpawnPoint spawnPoint = FindSpawnPoint(m_DestSpawnName, scene.name);
            if (spawnPoint != null)
                teleportable.TeleportTo(spawnPoint.transform);
            teleportable.CanTeleport = true;
        }

        private SpawnPoint FindSpawnPoint(string spawnName, string sceneName)
        {
            SpawnPoint[] spawnPoints = SceneUtility.FindComponentsInScene<SpawnPoint>(sceneName).ToArray();
            foreach (SpawnPoint spawn in spawnPoints)
            {
                SpawnPoint spawnPoint = spawn.GetComponent<SpawnPoint>();
                if (spawnPoint.spawnName == spawnName)
                    return spawnPoint;
            }
            return null;
        }
    }
}
