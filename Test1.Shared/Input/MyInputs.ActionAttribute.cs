using System;
using Ultraviolet.Input;
using System.Reflection;
using static System.Linq.Enumerable;
using System.Collections.Generic;

namespace test1.Input
{
    public static partial class MyInputs
    {
        [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        private sealed class ActionAttribute : Attribute
        {
            public ActionAttribute(Key key, string name)
            {
                Key = key;
                Name = name;
            }

            public Key Key { get; }
            public string Name { get; }

            public static void SetProperty<T, TAttribute, TProperty>(T @this, Action<TAttribute, TProperty>? getter = null, Func<TAttribute, TProperty>? setter = null)
                where TAttribute : Attribute
                where T : notnull
            {
                foreach (var field in GetBaseTypes(@this).Reverse().SelectMany(type => type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                    .Where(field => field.FieldType == typeof(TProperty)))
                {
                    var attribute = field.GetCustomAttribute<TAttribute>();
                    if (attribute is null)
                        continue;

                    if(getter is {} g)
                        g(attribute, (TProperty)field.GetValue(@this)!);
                    if(setter is {} s)
                        field.SetValue(@this, s(attribute));
                }

                static IEnumerable<Type> GetBaseTypes(T obj)
                {
                    for (Type? t = obj.GetType(); t != null && t != typeof(InputActionCollection); t = t.BaseType)
                        yield return t;
                }
            }
        }
    }
}
