using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAS.Pool
{
    public abstract class ComponentPoolSO<T> : PoolSO<T> where T : Component
    {
        private Transform _poolRoot;
        private Transform _parent;

        private Transform PoolRoot
        {
            get
            {
                if (_poolRoot == null)
                {
                    _poolRoot = new GameObject(name).transform;
                    var poolClearer = _poolRoot.AddComponent<PoolClearer>();
                    poolClearer.Init(this);
                    _poolRoot.SetParent(_parent);
                }

                return _poolRoot;
            }
        }

        public void SetSceneAndSetParent(Scene scene, Transform t)
        {
            _parent = t;
            PoolRoot.SetParent(_parent);
            SceneUtility.MoveGameObjectToScene(PoolRoot.gameObject, scene);
        }

        protected override bool Create(out T item)
        {
            if (base.Create(out item))
            {
                item.transform.SetParent(PoolRoot.transform);
                item.gameObject.SetActive(false);
                return true;
            }

            return false;
        }

        public override T Spawn(object data = null, MonoBase parent = null)
        {
            T item = base.Spawn(data, parent);
            item.gameObject.SetActive(true);
            return item;
        }

        public override void Despawn(T item)
        {
            item.transform.SetParent(PoolRoot.transform, false);
            item.gameObject.SetActive(false);
            base.Despawn(item);
        }

        public override void Clear()
        {
            foreach (var poolObject in Available)
            {
                if (poolObject != null)
                {
                    Destroy(poolObject.gameObject);
                }
            }

            base.Clear();
        }
    }
}