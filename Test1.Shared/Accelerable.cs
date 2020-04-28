using System.Collections.Generic;
using System.Linq;
using Ultraviolet;

namespace test1
{
    public abstract class Accelerable : Movable
    {

        protected readonly List<Vector2> _forces = new List<Vector2>();
        public IReadOnlyList<Vector2> Forces => _forces;
        public Vector2 Acceleration { get; protected set; }
        public Vector2? TemporaryForce { get; protected set; }

        public void AddConstantForce(Vector2 force)
        {
            _forces.Add(force);
            Acceleration = Forces.Aggregate(Ultraviolet.Vector2.Zero, (a, b) => a + b);
        }

        public void RemoveConstantForce(Vector2 force)
        {
            _forces.Remove(force);
            Acceleration = Forces.Aggregate(Ultraviolet.Vector2.Zero, (a, b) => a + b);
        }

        public void AddTemporaryForce(Vector2 force)
            => TemporaryForce = TemporaryForce.GetValueOrDefault() + force;

        public override void Update()
        {
            if (TemporaryForce is { } force)
                Velocity += force + Acceleration;
            else
                Velocity += Acceleration;
            TemporaryForce = null;
            base.Update();
        }
    }
}