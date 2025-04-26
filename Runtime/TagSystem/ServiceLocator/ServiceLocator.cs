using System;
using System.Collections.Generic;
using System.Linq;

namespace SAS.Utilities.TagSystem
{
    public class ServiceLocator : IServiceLocator
    {
        public interface IService
        {
        }

        private Dictionary<Key, List<object>> _services = new Dictionary<Key, List<object>>();

        public void Add<T>(object service, Tag tag = Tag.None)
        {
            Add(typeof(T), service, tag);
        }

        public void Add(Type type, object service, Tag tag = Tag.None)
        {
            var key = GetKey(type, tag);
            if (!_services.TryGetValue(key, out var serviceList))
            {
                serviceList = new List<object>();
                _services.Add(key, serviceList);
            }

            if (!serviceList.Contains(service))
                serviceList.Add(service);

            var baseTypes = type.GetInterfaces();
            if (type.BaseType != null)
                baseTypes = baseTypes.Prepend(type.BaseType).ToArray();

            foreach (var baseType in baseTypes)
                Add(baseType, service, tag);
        }

        private Key GetKey(Type type, Tag tag)
        {
            return new Key { type = type, tag = tag };
        }

        public T Get<T>(Tag tag = Tag.None)
        {
            TryGet<T>(out var service, tag);
            return service;
        }

        public bool TryGet<T>(out T service, Tag tag = Tag.None)
        {
            bool result = TryGet(typeof(T), out object serviceObj, tag);
            service = (T)serviceObj;
            return result;
        }

        public bool TryGet(Type type, out object service, Tag tag = Tag.None)
        {
            var key = GetKey(type, tag);
            if (!_services.TryGetValue(key, out var services))
            {
                service = null;
                Debug.LogError($"Required service of type {type.Name} with tag {tag} is not found");
                return false;
            }

            if (services.Count > 1)
                Debug.LogError($"There is more than one IService that implements {type.Name}");

            service = services[0];
            return true;
        }

        public IEnumerable<T> GetAll<T>(Tag tag = Tag.None)
        {
            return GetAll(typeof(T), tag).Cast<T>();
        }

        public IEnumerable<object> GetAll(Type type, Tag tag = Tag.None)
        {
            if (_services.TryGetValue(GetKey(type, tag), out var value))
                return value;
            else
                return Array.Empty<object>();
        }

        public T GetOrCreate<T>(Tag tag = Tag.None)
        {
            return (T)GetOrCreate(typeof(T), tag);
        }

        public object GetOrCreate(Type type, Tag tag = Tag.None)
        {
            var key = GetKey(type, tag);
            if (!_services.TryGetValue(key, out var values))
            {
                var instance = Activator.CreateInstance(type, new[] { this });
                Add(type, instance, tag);
                return instance;
            }

            return values[0];
        }

        public bool Remove<T>(Tag tag = Tag.None)
        {
            return Remove(typeof(T), tag);
        }

        public bool Remove(Type type, Tag tag = Tag.None)
        {
            var key = GetKey(type, tag);
            return !_services.Remove(key);
        }

        public void OnInstanceCreated()
        {
            Debug.Log("Service Loactor has been injected for very first time");
        }
    }
}