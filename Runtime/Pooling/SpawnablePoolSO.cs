using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.Pool
{
    [CreateAssetMenu(menuName = "SAS/Pool/Spawnable Object")]
    public class SpawnablePoolSO : ComponentPoolSO<Poolable>
    {
        [SerializeField] private FactorySO<Poolable> _factory = default;
        protected override IFactory<Poolable> Factory => _factory;

        protected override bool Create(out Poolable item)
        {
            if (base.Create(out item))
                item.ObjectPool = this;
            return item != null;
        }

        public override Poolable Spawn(object data = null, MonoBase parent = null)
        {
            var item = base.Spawn(data, parent);
            if (item != null)
            {
                item.SetParent(parent);
                item.active = true;
            }
            return item;
        }

        public override void Despawn(Poolable item)
        {
            if (!item.active)
                return;
            var children = item?.Children;
            for (int i = children.Count - 1; i >= 0; i--)
            {
                (children[i] as Poolable).Despawn();
            }
            item.active = false;
            item.Unparent();
            base.Despawn(item);
        }
    }
}

