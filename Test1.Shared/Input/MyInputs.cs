using System;
using Ultraviolet;
using Ultraviolet.Input;

namespace test1.Input
{
    public static partial class MyInputs
    {
        public static GlobalActions GetGlobalActions(this IUltravioletInput input) =>
            GlobalActions.Instance;

        public static ShipActions GetShipActions(this IUltravioletInput input) =>
            ShipActions.Instance;

        public static NotInShipActions GetNotInShipActions(this IUltravioletInput input) =>
            NotInShipActions.Instance;

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

        public sealed class GlobalActions : Actions<GlobalActions>
        {
            public GlobalActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.R, "RESTART")]
            public InputAction RestartApplication { get; } = null!;

            [field: Action(Key.Tab, "NEXT_PLANET")]
            public InputAction NextPlanet { get; } = null!;

            [field: Action(Key.Escape, "DESELECT_PLANET")]
            public InputAction DeselectPlanet { get; } = null!;

            [field: Action(Key.Space, "PLAY_SIMULATION")]
            public InputAction PlaySimulation { get; } = null!;

            [field: Action(Key.Return, "STEP_SIMULATION")]
            public InputAction StepSimulation { get; } = null!;

            [field: Action(Key.Backspace, "SHOW_GRAVTY")]
            public InputAction ShowGravity { get; } = null!;

            [field: Action(Key.D1, "TIME_RATIO_1")]
            public InputAction TimeRatio1 { get; } = null!;

            [field: Action(Key.D2, "TIME_RATIO_2")]
            public InputAction TimeRatio2 { get; } = null!;

            [field: Action(Key.D3, "TIME_RATIO_3")]
            public InputAction TimeRatio3 { get; } = null!;

            [field: Action(Key.D4, "TIME_RATIO_4")]
            public InputAction TimeRatio4 { get; } = null!;
        }

        public sealed class NotInShipActions : MovementActions<NotInShipActions>
        {
            public NotInShipActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.Delete, "DELETE_PLANET")]
            public InputAction DeletePlanet { get; } = null!;

            [field: Action(Key.RightControl, "ENTER_SHIP")]
            public InputAction EnterShip { get; } = null!;
        }

        public sealed class ShipActions : Actions<ShipActions>
        {
            public ShipActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.RightControl, "EXIT_SHIP")]
            public InputAction ExitShip { get; } = null!;

            [field: Action(Key.Up, "FORWARD")]
            public InputAction Forward { get; } = null!;

            [field: Action(Key.Down, "BACKWARD")]
            public InputAction Backward { get; } = null!;

            [field: Action(Key.Left, "LEFT")]
            public InputAction Left { get; } = null!;

            [field: Action(Key.Right, "RIGHT")]
            public InputAction Right { get; } = null!;
        }
    }
}
