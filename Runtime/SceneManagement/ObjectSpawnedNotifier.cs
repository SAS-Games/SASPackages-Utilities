using System;
using SAS.SceneManagement;
using System.Linq;
using UnityEngine;

public class ObjectSpawnedNotifier : MonoBehaviour
{
    private EventBinding<SceneGroupLoadedEvent> _sceneGroupLoadedBinding;
    private EventBinding<AdditiveSceneLoadedEvent> _additiveSceneLoadedBinding;


    private void Awake()
    {
        _sceneGroupLoadedBinding = new EventBinding<SceneGroupLoadedEvent>(groupLoadedEventData =>
        {
            var group = groupLoadedEventData.sceneGroup;
            foreach (var scene in group.Scenes)
            {
                var listeners = SceneUtility.FindComponentsInScene<IObjectSpawnedListener>(scene.Name);
                foreach (var listener in listeners)
                    listener.OnSpawn(gameObject);
            }
        });

        _additiveSceneLoadedBinding = new EventBinding<AdditiveSceneLoadedEvent>(additiveSceneLoadedEvent =>
        {
            var listeners = SceneUtility.FindComponentsInScene<IObjectSpawnedListener>(additiveSceneLoadedEvent.scene.name);
            foreach (var listener in listeners)
                listener.OnSpawn(gameObject);
        });
        
        EventBus<SceneGroupLoadedEvent>.Register(_sceneGroupLoadedBinding);
        EventBus<AdditiveSceneLoadedEvent>.Register(_additiveSceneLoadedBinding);
    }

    private void OnDestroy()
    {
        EventBus<SceneGroupLoadedEvent>.Deregister(_sceneGroupLoadedBinding);
        EventBus<AdditiveSceneLoadedEvent>.Deregister(_additiveSceneLoadedBinding);
    }
}
