namespace test1.Input
{
    public static partial class MyInputs
    {
        public sealed class InputAction
        {
            public enum ClicType
            {
                Down,
                Up,
                Pressed,
                PressedRepeated,
                Released
            }

            private readonly Ultraviolet.Input.InputAction _action;
            private readonly ClicType _type;

            public InputAction(Ultraviolet.Input.InputAction action, ClicType type)
            {
                _action = action;
                _type = type;
            }

            public bool IsActionned
                => _type switch
                {
                    ClicType.Down => _action.IsDown(),
                    ClicType.Up => _action.IsUp(),
                    ClicType.Pressed => _action.IsPressed(ignoreRepeats: true),
                    ClicType.PressedRepeated => _action.IsPressed(ignoreRepeats: false),
                    ClicType.Released => _action.IsReleased(),
                };
            
            public Ultraviolet.Input.InputBinding Primary
            {
                get => _action.Primary;
                set => _action.Primary = value;
            }
            
            public static implicit operator bool(InputAction action)
                => action.IsActionned;
        }
    }
}
