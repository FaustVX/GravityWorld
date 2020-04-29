using System.Collections.Generic;
using System.Linq;
using Ultraviolet;

namespace test1
{
    public abstract class Accelerable : Movable
    {
        public Vector2 Acceleration { get; protected set; } = Vector2.Zero;

        public void ResetAcceleration()
            => Acceleration = Vector2.Zero;

        public virtual void AddTemporaryForce(Vector2 force)
            => Acceleration += force;

        public override void Update()
        {
            Velocity += Acceleration;
            base.Update();
        }
    }
}