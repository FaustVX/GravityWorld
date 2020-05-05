using System;
using Ultraviolet;
using Ultraviolet.Input;
using ClicType = test1.Input.MyInputs.InputAction.ClicType;

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

            [field: Action(Key.Up, "UP", ClicType.Down)]
            public InputAction Up { get; } = null!;

            [field: Action(Key.Down, "DOWN", ClicType.Down)]
            public InputAction Down { get; } = null!;

            [field: Action(Key.Left, "LEFT", ClicType.Down)]
            public InputAction Left { get; } = null!;

            [field: Action(Key.Right, "RIGHT", ClicType.Down)]
            public InputAction Right { get; } = null!;
        }

        public sealed class GlobalActions : Actions<GlobalActions>
        {
            public GlobalActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.R, "RESTART", ClicType.Pressed)]
            public InputAction RestartApplication { get; } = null!;

            [field: Action(Key.Tab, "NEXT_PLANET", ClicType.Pressed)]
            public InputAction NextPlanet { get; } = null!;

            [field: Action(Key.Escape, "DESELECT_PLANET", ClicType.Pressed)]
            public InputAction DeselectPlanet { get; } = null!;

            [field: Action(Key.Space, "PLAY_SIMULATION", ClicType.Pressed)]
            public InputAction PlaySimulation { get; } = null!;

            [field: Action(Key.Return, "STEP_SIMULATION", ClicType.PressedRepeated)]
            public InputAction StepSimulation { get; } = null!;

#if DEBUG
            [field: Action(Key.Backspace, "SHOW_GRAVTY", ClicType.Up)]
#else
            [field: Action(Key.Backspace, "SHOW_GRAVTY", ClicType.Down)]
#endif
            public InputAction ShowGravity { get; } = null!;

            [field: Action(Key.D1, "TIME_RATIO_1", ClicType.Pressed)]
            public InputAction TimeRatio1 { get; } = null!;

            [field: Action(Key.D2, "TIME_RATIO_2", ClicType.Pressed)]
            public InputAction TimeRatio2 { get; } = null!;

            [field: Action(Key.D3, "TIME_RATIO_3", ClicType.Pressed)]
            public InputAction TimeRatio3 { get; } = null!;

            [field: Action(Key.D4, "TIME_RATIO_4", ClicType.Pressed)]
            public InputAction TimeRatio4 { get; } = null!;
        }

        public sealed class NotInShipActions : MovementActions<NotInShipActions>
        {
            public NotInShipActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.Delete, "DELETE_PLANET", ClicType.Pressed)]
            public InputAction DeletePlanet { get; } = null!;

            [field: Action(Key.RightControl, "ENTER_SHIP", ClicType.Pressed)]
            public InputAction EnterShip { get; } = null!;
        }

        public sealed class ShipActions : Actions<ShipActions>
        {
            public ShipActions(UltravioletContext uv)
                : base(uv)
            { }

            [field: Action(Key.RightControl, "EXIT_SHIP", ClicType.Pressed)]
            public InputAction ExitShip { get; } = null!;

            [field: Action(Key.Up, "FORWARD", ClicType.Down)]
            public InputAction Forward { get; } = null!;

            [field: Action(Key.Down, "BACKWARD", ClicType.Down)]
            public InputAction Backward { get; } = null!;

            [field: Action(Key.Left, "LEFT", ClicType.Down)]
            public InputAction Left { get; } = null!;

            [field: Action(Key.Right, "RIGHT", ClicType.Down)]
            public InputAction Right { get; } = null!;
        }
    }
}
