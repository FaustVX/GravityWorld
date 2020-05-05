using Ultraviolet;
using Ultraviolet.Input;

namespace test1.Input
{
    public static partial class MyInputs
    {
        public abstract class Actions<T> : InputActionCollection
            where T : Actions<T>
        {
            protected Actions(UltravioletContext uv)
                : base(uv)
            { }

            public static T Instance { get; } = CreateSingleton<T>();

            /// <inheritdoc/>
            protected override void OnCreatingActions()
            {
                ActionAttribute.SetProperty<T, ActionAttribute, InputAction>((T)this, setter: (ActionAttribute attribut) => new InputAction(CreateAction(attribut.Name), attribut.ClicType));

                base.OnCreatingActions();
            }

            /// <inheritdoc/>
            protected override void OnResetting()
            {
                switch (Ultraviolet.Platform)
                {
                    default:
                        Reset_Desktop();
                        break;
                }
                base.OnResetting();
            }

            private void Reset_Desktop()
                => ActionAttribute.SetProperty(this, getter: (ActionAttribute attribute, InputAction action) => action.Primary = CreateKeyboardBinding(attribute.Key));
        }
    }
}
