﻿using System.Collections.Generic;
using UnityEngine;

namespace SAS.Utilities.TagSystem
{
    public class MonoBase : MonoBehaviour
    {
        private List<MonoBase> _children = new List<MonoBase>();
        public IReadOnlyList<MonoBase> Children => _children;
        private MonoBase _parent;


        protected virtual void OnDestroy()
        {
            // TODO: Use refection set sett all the properties null;
            foreach (var child in _children)
            {
                Destroy(child.gameObject);
            }
        }

        internal void SetParent(MonoBase parent)
        {
            _parent = parent;
            parent?.AddChild(this);
        }

        private void AddChild(MonoBase monoBase)
        {
            _children.Add(monoBase);
        }

        internal void Unparent()
        {
            _parent?._children?.Remove(this);
            _parent = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            this.Initialize();
        }
#endif
    }
}
