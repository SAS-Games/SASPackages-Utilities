using UnityEngine;

namespace SAS.Pool
{
    [CreateAssetMenu(menuName = "SAS/Factory/Spawnable Object")]
    public class SpawnableFactorySO : FactorySO<Poolable>
    {
        [SerializeField] private GameObject m_Prefab = default;

        public override bool Create(out Poolable item)
        {
            item = Instantiate(m_Prefab).GetComponent<Poolable>();
             if(item == null) 
                 Debug.LogError($"Object is not having the Poolable component attached, factory is {this.name}");
            return item != null;
        }
    }
}
