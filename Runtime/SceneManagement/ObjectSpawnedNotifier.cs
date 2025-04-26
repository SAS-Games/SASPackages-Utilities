using SAS.SceneManagement;
using System.Linq;
using UnityEngine;

public class ObjectSpawnedNotifier : MonoBehaviour
{
    private void Awake()
    {
        EventBus<SceneGroupLoadedEvent>.Register(new EventBinding<SceneGroupLoadedEvent>(groupLoadedEventData =>
        {
            var group = groupLoadedEventData.sceneGroup;
            foreach (var scene in group.Scenes)
            {
                var listeners = SceneUtility.FindComponentsInScene<IObjectSpawnedListener>(scene.Name);
                foreach (var listener in listeners)
                    listener.OnSpawn(gameObject);
            }
        }));

        EventBus<AdditiveSceneLoadedEvent>.Register(new EventBinding<AdditiveSceneLoadedEvent>(additiveSceneLoadedEvent =>
        {
            var listeners = SceneUtility.FindComponentsInScene<IObjectSpawnedListener>(additiveSceneLoadedEvent.scene.name);
            foreach (var listener in listeners)
                listener.OnSpawn(gameObject);
        }));
    }
}
