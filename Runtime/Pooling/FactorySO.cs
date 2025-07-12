using UnityEngine;

namespace SAS.Pool
{
    public abstract class FactorySO<T> : ScriptableObject, IFactory<T>
    {
        public abstract bool Create(string id, out T item);
    }
}
