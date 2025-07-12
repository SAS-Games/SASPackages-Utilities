using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.Pool
{
    public interface IFactory<T>
    {
        bool Create(string id, out T item);
    }
}