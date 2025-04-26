using System;
using System.Collections.Generic;

namespace SAS.Utilities.TagSystem
{
    public interface IServiceLocator : IBindable
    {
        void Add<T>(object service, Tag tag = Tag.None);
        void Add(Type type, object service, Tag tag = Tag.None);
        T Get<T>(Tag tag = Tag.None);
        bool TryGet<T>(out T service, Tag tag = Tag.None);
        bool TryGet(Type type, out object service, Tag tag = Tag.None);
        IEnumerable<T> GetAll<T>(Tag tag = Tag.None);
        T GetOrCreate<T>(Tag tag = Tag.None);
    }
}