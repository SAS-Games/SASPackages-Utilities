﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAS.Utilities.TagSystem
{
    public static class ComponentExtensions
    {
        private readonly static Dictionary<Type, Func<Component, Type, Tag, Component>> _componentCreator = new Dictionary<Type, Func<Component, Type, Tag, Component>>
        {
            { typeof(InjectAttribute), (comp, type, tag) => comp.AddComponent(type, tag) },
        };

        private static readonly Dictionary<Type, Func<Component, Type, bool, Component>> _componentFetchers = new Dictionary<Type, Func<Component, Type, bool, Component>>
        {
            { typeof(FieldRequiresSelfAttribute), (comp, type, includeInactive) => comp.GetComponent(type) },
            { typeof(FieldRequiresChildAttribute), (comp, type, includeInactive) => comp.GetComponentInChildren(type, includeInactive) },
            { typeof(FieldRequiresParentAttribute), (comp, type, includeInactive) => comp.GetComponentInParent(type) },
        };

        private static readonly Dictionary<Type, Func<Component, Type, Tag, bool, Component>> _componentWithTagFetchers = new Dictionary<Type, Func<Component, Type, Tag, bool, Component>>
        {
            { typeof(FieldRequiresSelfAttribute), (comp, type, tag, includeInactive) => comp.GetComponent(type, tag) },
            { typeof(FieldRequiresChildAttribute), (comp, type, tag, includeInactive) => comp.GetComponentInChildren(type, tag, includeInactive) },
            { typeof(FieldRequiresParentAttribute), (comp, type, tag, includeInactive) => comp.GetComponentInParent(type, tag, includeInactive) },
        };

        private static readonly Dictionary<Type, Func<Component, Type, bool, Component[]>> _componentsFetchers = new Dictionary<Type, Func<Component, Type, bool, Component[]>>
        {
             { typeof(FieldRequiresSelfAttribute), (comp, type, includeInactive) => comp.GetComponents(type) },
             { typeof(FieldRequiresChildAttribute), (comp, type, includeInactive) => comp.GetComponentsInChildren(type, includeInactive) },
             { typeof(FieldRequiresParentAttribute), (comp, type, includeInactive) => comp.GetComponentsInParent(type, includeInactive) },
        };

        private static Dictionary<Type, Func<Component, Type, Tag, bool, Component[]>> _componentsWithTagFetchers = new Dictionary<Type, Func<Component, Type, Tag, bool, Component[]>>
        {
             { typeof(FieldRequiresSelfAttribute), (comp, type, tag, includeInactive) => comp.GetComponents(type, tag) },
             { typeof(FieldRequiresChildAttribute), (comp, type, tag, includeInactive) => comp.GetComponentsInChildren(type, tag, includeInactive) },
             { typeof(FieldRequiresParentAttribute), (comp, type, tag, includeInactive) => comp.GetComponentsInParent(type, tag, includeInactive) },
        };

        internal static Dictionary<string, IContextBinder> _cachedContext = new Dictionary<string, IContextBinder>();

        public static void Initialize(this Component component, object instance = null)
        {
            instance = instance ?? component;
            var allFields = GetAllFields(instance);
            TryGetContext(component.gameObject, out var context);

            foreach (var field in allFields)
            {
                var requirement = field.GetCustomAttribute<BaseRequiresAttribute>(false);
                if (requirement != null)
                {

                    if (requirement is BaseRequiresComponent)
                    {
                        var componentRequirement = requirement as BaseRequiresComponent;
                        if (field.FieldType.IsArray)
                        {
                            var elementType = field.FieldType.GetElementType();
                            var dependencies = default(Component[]);
                            if (requirement.tag == Tag.None)
                                dependencies = _componentsFetchers[requirement.GetType()](component, elementType, componentRequirement.includeInactive);
                            else
                                dependencies = _componentsWithTagFetchers[requirement.GetType()](component, elementType, requirement.tag, componentRequirement.includeInactive);

                            field.SetValue(instance, ConvertArray(dependencies, elementType));
                        }
                        else
                        {
                            var dependency = default(Component);
                            if (requirement.tag == Tag.None)
                                dependency = _componentFetchers[requirement.GetType()](component, field.FieldType, componentRequirement.includeInactive);
                            else
                                dependency = _componentWithTagFetchers[requirement.GetType()](component, field.FieldType, requirement.tag, componentRequirement.includeInactive);
                            field.SetValue(instance, dependency);
                        }
                    }
                    else if (requirement is InjectAttribute)
                    {
                        var modelRequirement = requirement as InjectAttribute;
                        Inject(context, instance, field, modelRequirement);
                    }
                }
            }
        }

        private static IEnumerable<FieldInfo> GetAllFields(this object instance)
        {
            var instanceType = instance.GetType();
            var baseType = instanceType.BaseType;

            FieldInfo[] fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var enumerable = fields.AsEnumerable();

            while (baseType != null)
            {
                var baseFields = baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                enumerable = enumerable.Concat(baseFields);
                baseType = baseType.BaseType;
            }

            return fields.AsEnumerable();
        }

        private static Array ConvertArray<T>(T[] elements, Type castType)
        {
            var array = Array.CreateInstance(castType, elements.Length);
            for (var i = 0; i < elements.Length; ++i)
                array.SetValue(elements[i], i);

            return array;
        }

        private static bool TryGetContext(GameObject gameObject, out IContextBinder context)
        {
            if (!_cachedContext.TryGetValue(gameObject.scene.name, out context))
            {
                var scene = gameObject.scene;
                var rootObjects = scene.GetRootGameObjects();
                foreach (var rootObject in rootObjects)
                {
                    if (rootObject.TryGetComponent(out context))
                    {
                        _cachedContext[scene.name] = context;
                        return true;
                    }

                }
            }
            return false;
        }

        private static void Inject(IContextBinder context, object instance, FieldInfo field, InjectAttribute requirement)
        {
            if (context == null)
            {
                InjectCrossContext(instance, field, requirement);
                return;
            }
            if (requirement.optional)
            {
                if (context.TryGet(field.FieldType, out var obj))
                    field.SetValue(instance, obj);
            }
            else
            {
                var value = context.GetOrCreate(field.FieldType, requirement.tag);
                if (value != null)
                    field.SetValue(instance, value);
                else
                    InjectCrossContext(instance, field, requirement);
            }
        }

        private static void InjectCrossContext(object instance, FieldInfo field, InjectAttribute requirement)
        {
            if (_cachedContext.TryGetValue("DontDestroyOnLoad", out var crossContext))
            {
                if (requirement.optional)
                {
                    if (crossContext.TryGet(field.FieldType, out var obj))
                        field.SetValue(instance, obj);
                }
                else
                {
                   var value = crossContext.GetOrCreate(field.FieldType, requirement.tag);
                    if (value != null)
                        field.SetValue(instance, value);
                }
            }
        }
    }
}

