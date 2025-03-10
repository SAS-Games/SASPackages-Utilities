using SAS.Utilities.TagSystem;

namespace SAS.Pool
{
    public abstract class Poolable : MonoBase
    {
        internal ComponentPoolSO<Poolable> ObjectPool { get; set; }
        public bool active { get; private set; } = false;
       

        public void Despawn()
        {
            if (ObjectPool != null)
                ObjectPool.Despawn(this);
        }
    }
}
