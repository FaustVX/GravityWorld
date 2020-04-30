using Ultraviolet;
using Ultraviolet.Graphics;

namespace test1
{
    public class Mover : Accelerable
    {
        public Mover(int mass)
        {
            Mass = mass;
        }
        public int Mass { get; protected set; }

        public Texture2D Texture { get; set; } = null!;

        public override void AddTemporaryForce(Vector2 force)
            => base.AddTemporaryForce(force / Mass);
    }
}