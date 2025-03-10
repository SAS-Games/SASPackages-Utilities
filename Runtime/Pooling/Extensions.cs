using System;
using UnityEngine;

namespace SAS.Pool
{
    public static class Extensions
    {
        public static void ForEachChild<T>(this GameObject gameObject, Action<T> childAction, bool includeInactive = true)
        {
            foreach (var child in gameObject.GetComponentsInChildren<T>(includeInactive))
            {
                childAction(child);
            }
        }

        public static void ForEachChild<T>(this Component component, Action<T> childAction, bool includeInactive = true)
        {
            component.gameObject.ForEachChild<T>(childAction, includeInactive);
        }

        public static void ForEachChild<T>(this object item, Action<T> childAction, bool includeInactive = true)
        {
            GameObject gameObject = item switch
            {
                GameObject go => go,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject != null)
            {
                gameObject.ForEachChild(childAction, includeInactive);
            }
            else
            {
                Debug.LogError($"Cannot convert {typeof(T)} to GameObject.");
            }
        }
    }
}