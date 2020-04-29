using System;
using Ultraviolet;
using Ultraviolet.Input;

namespace test1.Input
{
    public static partial class MyInputs
    {
        public static GlobalActions GetGlobalActions(this IUltravioletInput input) =>
            GlobalActions.Instance;

        public abstract class MovementActions<T> : Actions<T>
            where T : MovementActions<T>
        {
            protected MovementActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.Up, "UP")]
            public InputAction Up { get; } = null!;

            [field: Action(Key.Down, "DOWN")]
            public InputAction Down { get; } = null!;

            [field: Action(Key.Left, "LEFT")]
            public InputAction Left { get; } = null!;

            [field: Action(Key.Right, "RIGHT")]
            public InputAction Right { get; } = null!;
        }

        public sealed class GlobalActions : MovementActions<GlobalActions>
        {
            public GlobalActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.Escape, "EXIT")]
            public InputAction ExitApplication { get; } = null!;

            [field: Action(Key.R, "RESTART")]
            public InputAction RestartApplication { get; } = null!;

            [field: Action(Key.Tab, "NEXT_PLANET")]
            public InputAction NextPlanet { get; } = null!;

            [field: Action(Key.Delete, "DELETE_PLANET")]
            public InputAction DeletePlanet { get; } = null!;

            [field: Action(Key.Space, "PLAY_SIMULATION")]
            public InputAction PlaySimulation { get; } = null!;

            [field: Action(Key.Return, "STEP_SIMULATION")]
            public InputAction StepSimulation { get; } = null!;

            [field: Action(Key.Backspace, "SHOW_GRAVTY")]
            public InputAction ShowGravity { get; } = null!;
        }
    }
}
