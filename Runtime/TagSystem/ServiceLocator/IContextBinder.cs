﻿using System;
using System.Collections.Generic;

namespace SAS.Utilities.TagSystem
{
    public interface IContextBinder
    {
        object GetOrCreate(Type type, Tag tag = Tag.None);
        bool TryGet(Type type, out object instance, Tag tag = Tag.None);
        bool TryGet<T>(out T instance, Tag tag = Tag.None);

        void Add(Type type, object instance, Tag tag = Tag.None);
        IReadOnlyDictionary<Key, object> GetAll();
    }
}
