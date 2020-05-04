using Ultraviolet.Graphics;

namespace test1
{
    public static class Globals
    {
        public static int TimeRatio { get; set; } = 1;
        public static Texture2D Pixel { get; set; } = null!;
        public static float G { get; } = .03f;
        private static readonly int _initialMaxPlanets = 1000;
        public static int MaxPlanets1 { get; set; } = _initialMaxPlanets;
        public static int MaxPlanets2 { get; set; } = _initialMaxPlanets;
        public static int MaxPlanets3 { get; set; } = _initialMaxPlanets;
        public static int MaxPlanets4 { get; set; } = _initialMaxPlanets;
        public static float MinimumGravityForCalculation{ get; } = .05f;
    }
}
