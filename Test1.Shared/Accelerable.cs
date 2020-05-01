using System.Collections.Generic;
using System.Linq;
using Ultraviolet;

namespace test1
{
    public abstract class Accelerable : Movable
    {
        public Vector2 Acceleration { get; set; } = Vector2.Zero;
        public float AngleAcceleration { get; set; } = 0f;

        public void ResetAccelerations()
        {
            Acceleration = Vector2.Zero;
            AngleAcceleration = 0;
        }

        public virtual void AddTemporaryForce(Vector2 force)
            => Acceleration += force;

        public override void Update(UltravioletTime time)
        {
            Velocity += Acceleration * (float)time.ElapsedTime.TotalSeconds;
            AngleVelocity += AngleAcceleration * (float)time.ElapsedTime.TotalSeconds;
            base.Update(time);
        }
    }
}