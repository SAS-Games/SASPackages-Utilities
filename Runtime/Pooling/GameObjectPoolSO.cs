using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.Pool
{
    [CreateAssetMenu(menuName = "SAS/Pool/GameObject Pool")]
    public class GameObjectPoolSO : PoolSO<GameObject>
    {
        [SerializeField] private GameObject _prefab;

        public GameObject Prefab => _prefab;
        protected override IFactory<GameObject> Factory => throw new System.NotImplementedException();
        private Transform _poolRoot;
        private Transform _parent;

        private Transform PoolRoot
        {
            get
            {
                if (_poolRoot == null)
                {
                    _poolRoot = new GameObject(name).transform;
                    _poolRoot.SetParent(_parent);
                }
                return _poolRoot;
            }
        }


        public void SetParent(Transform t)
        {
            _parent = t;
            PoolRoot.SetParent(_parent);
        }

        protected override bool Create(out GameObject item)
        {
            item = Instantiate(_prefab);
            item.transform.SetParent(PoolRoot.transform);
            item.gameObject.SetActive(false);
            return true;
        }

        public override GameObject Spawn(object data = null, MonoBase parent = null)
        {
            GameObject item = base.Spawn(data, parent);
            item.SetActive(true);
            return item;
        }

        public override void Despawn(GameObject item)
        {
            item.transform.SetParent(PoolRoot, false);
            item.SetActive(false);
            base.Despawn(item);
        }

        public override void Clear()
        {
            foreach (var poolObject in Available)
            {
                if (poolObject != null)
                {
                    Destroy(poolObject);
                }
            }
            base.Clear();
        }
    }
}
