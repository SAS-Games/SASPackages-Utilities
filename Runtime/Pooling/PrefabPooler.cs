using System.Collections.Generic;
using UnityEngine;

namespace SAS.Pool
{

    public class PrefabPooler
    {
        private Dictionary<GameObject, GameObjectPoolSO> _prefabPools;
        private Dictionary<int, GameObject> _prefabInstanceLookup;

        public PrefabPooler()
        {
            _prefabPools = new Dictionary<GameObject, GameObjectPoolSO>();
            _prefabInstanceLookup = new Dictionary<int, GameObject>();
        }

        public GameObject Spawn(GameObjectPoolSO gameObjectPoolSO)
        {
            var prefab = gameObjectPoolSO.Prefab;
            if (!_prefabPools.ContainsKey(prefab))
            {
                Debug.LogError("Tried to spawn unregistered prefab from the prefab pool");
                return null;
            }

            var spawnedObject = _prefabPools[prefab].Spawn();
            _prefabInstanceLookup.Add(spawnedObject.GetInstanceID(), prefab);
            return spawnedObject;
        }

        public void Despawn(GameObject spawnedObject)
        {
            var originalPrefab = _prefabInstanceLookup[spawnedObject.GetInstanceID()];
            if (originalPrefab == null || !_prefabPools.ContainsKey(originalPrefab))
            {
                Debug.LogError("Tried to despawn unregistered prefab from the prefab pool.");
                return;
            }

            _prefabInstanceLookup.Remove(spawnedObject.GetInstanceID());
            _prefabPools[originalPrefab].Despawn(spawnedObject);
        }

        public void AllocatePool(GameObjectPoolSO prefabPool, int initialSize = 4)
        {
            if (_prefabPools.ContainsKey(prefabPool.Prefab))
            {
                return;
            }

            prefabPool.Initialize(initialSize);
            _prefabPools.Add(prefabPool.Prefab, prefabPool);
        }
    }
}