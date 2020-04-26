using Ultraviolet;
using Ultraviolet.Core;
using Ultraviolet.Input;

namespace test1.Input
{
    public static class MyInputs
    {
        public static Actions GetActions(this IUltravioletInput input) =>
            Actions.Instance;

        public sealed class Actions : InputActionCollection
        {
            public Actions(UltravioletContext uv)
                : base(uv)
            { }

            public static Actions Instance { get; } = CreateSingleton<Actions>();

            public InputAction ExitApplication { get; private set; }

            /// <inheritdoc/>
            protected override void OnCreatingActions()
            {
                ExitApplication = CreateAction("EXIT_APPLICATION");

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
            {
                ExitApplication.Primary = CreateKeyboardBinding(Key.Escape);
            }

            private void Reset_Android()
            {
                ExitApplication.Primary = CreateKeyboardBinding(Key.AppControlBack);
            }
        }
    }
}
