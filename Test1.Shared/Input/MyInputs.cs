using Ultraviolet;
using Ultraviolet.Core;
using Ultraviolet.Input;
using System.Reflection;
using static System.Linq.Enumerable;

namespace test1.Input
{
    public static class MyInputs
    {
        [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        private sealed class ActionAttribute : System.Attribute
        {
            public ActionAttribute(Key key, string name)
            {
                Key = key;
                Name = name;
            }

            public Key Key { get; }
            public string Name { get; }

            public static void SetProperty<T, TAttribute, TProperty>(T @this, System.Action<TAttribute, TProperty>? getter = null, System.Func<TAttribute, TProperty>? setter = null)
                where TAttribute : System.Attribute
                where T : notnull
            {
                foreach (var property in @this.GetType().GetProperties()
                    .Where(prop => prop.PropertyType == typeof(TProperty)))
                {
                    var attribute = property.GetCustomAttribute<TAttribute>();
                    if (attribute is null)
                        continue;
                    
                    if(getter is {} g)
                        g(attribute, (TProperty)property.GetValue(@this)!);
                    if(setter is {} s)
                        property.SetValue(@this, s(attribute));
                }
            }
        }

        public static GlobalActions GetGlobalActions(this IUltravioletInput input) =>
            GlobalActions.Instance;

        public sealed class GlobalActions : InputActionCollection
        {
            public GlobalActions(UltravioletContext uv)
                : base(uv)
            { }

            public static GlobalActions Instance { get; } = CreateSingleton<GlobalActions>();

            [Action(Key.Escape, "EXIT")]
            public InputAction ExitApplication { get; private set; } = null!;

            [Action(Key.R, "RESTART")]
            public InputAction RestartApplication { get; private set; } = null!;

            [Action(Key.Up, "UP")]
            public InputAction Up { get; private set; } = null!;

            [Action(Key.Down, "DOWN")]
            public InputAction Down { get; private set; } = null!;

            [Action(Key.Left, "LEFT")]
            public InputAction Left { get; private set; } = null!;

            [Action(Key.Right, "RIGHT")]
            public InputAction Right { get; private set; } = null!;

            /// <inheritdoc/>
            protected override void OnCreatingActions()
            {
                ActionAttribute.SetProperty(this, setter: (ActionAttribute attribut) => CreateAction(attribut.Name));

                base.OnCreatingActions();
            }

            /// <inheritdoc/>
            protected override void OnResetting()
            {
                switch (Ultraviolet.Platform)
                {
                    case UltravioletPlatform.Android:
                        Reset_Android();
                        break;

                    default:
                        Reset_Desktop();
                        break;
                }
                base.OnResetting();
            }

            private void Reset_Desktop()
                => ActionAttribute.SetProperty(this, getter: (ActionAttribute attribute, InputAction action) => action.Primary = CreateKeyboardBinding(attribute.Key));

            private void Reset_Android()
            {
                ExitApplication.Primary = CreateKeyboardBinding(Key.AppControlBack);
            }
        }
    }
}
