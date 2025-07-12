using UnityEngine;
using System.Collections.Generic;

namespace SAS.Pool
{
    public abstract class VariantFactorySO<T> : FactorySO<T> where T : Component
    {
        [System.Serializable]
        private class Entry
        {
            public string PoolId;
            public GameObject Prefab;
        }

        [SerializeField] private List<Entry> _prefabs = new();

        private Dictionary<string, GameObject> _prefabMap;

        protected virtual void OnEnable()
        {
            _prefabMap = new Dictionary<string, GameObject>();
            foreach (var entry in _prefabs)
            {
                if (!string.IsNullOrEmpty(entry.PoolId) && entry.Prefab != null)
                    _prefabMap[entry.PoolId] = entry.Prefab;
            }
        }

        public override bool Create(string id, out T item)
        {
            item = null;

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError($"[{name}] PoolID is null or empty.");
                return false;
            }

            if (!_prefabMap.TryGetValue(id, out var prefab))
            {
                Debug.LogError($"[{name}] No prefab found for PoolID: {id}");
                return false;
            }

            item = Instantiate(prefab).GetComponent<T>();
            if (item == null)
            {
                Debug.LogError($"[{name}] Prefab for PoolID '{id}' does not have a {typeof(T).Name} component.");
                return false;
            }

            return true;
        }
    }
}