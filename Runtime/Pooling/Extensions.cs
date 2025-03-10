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
            if (item is GameObject go)
                go.ForEachChild(childAction, includeInactive);
            else if (item is Component component)
                component.gameObject.ForEachChild(childAction, includeInactive);
            else
                Debug.LogError($"ForEachChild: Cannot convert {item?.GetType().Name ?? "null"} to GameObject.");
        }
    }
}