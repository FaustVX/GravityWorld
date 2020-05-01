using Ultraviolet;

namespace test1
{
    public abstract class Movable : Positionable
    {
        public Vector2 Velocity { get; set; }
        public float Friction { get; set; } = 1f;

        public float AngleVelocity { get; set; }
        public float AngleFriction { get; set; } = 1f;

        public virtual void Update(UltravioletTime time)
        {
            Position += Velocity * (float)time.ElapsedTime.TotalSeconds;
            Velocity *= Friction;

            Rotation += AngleVelocity * (float)time.ElapsedTime.TotalSeconds;
            AngleVelocity *= AngleFriction;
        }
    }
}
