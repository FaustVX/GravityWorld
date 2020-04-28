using Ultraviolet;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public class Mover : Accelerable
    {
        public Mover(int radius)
        {
            Radius = radius;
        }

        private int _radius;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                _mass = (int)(System.MathF.PI * Radius * Radius);
            }
        }

        public int Diametre
        {
            get => Radius * 2;
            set => Radius = value / 2;
        }

        private int _mass;
        public int Mass
        {
            get => _mass;
            set
            {
                _mass = value;
                _radius = (int)(System.MathF.Sqrt(Mass / System.MathF.PI));
            }
        }

        public Texture2D Texture { get; set; } = null!;

        public virtual void Attract(Mover other)
        {
            Vector2 vector, force;
            CalculateGravity(other.Position, other.Mass, out vector, out force);
            if (vector.Length() <= other.Radius + Radius)
            {
                var normal = Vector2.Normalize(vector);
                var reflected = Vector2.Reflect(other.Velocity, normal);
                other.Velocity = reflected;
            }

            other.AddTemporaryForce(force);
        }

        public virtual void CalculateGravity(Vector2 position, int mass, out Vector2 vector, out Vector2 force)
        {
            var G = .025f;
            vector = Position - position;
            var lengthSquared = vector.LengthSquared();
            force = Vector2.Normalize(vector) * (G * (mass * Mass) / lengthSquared);
        }

        public virtual void Draw(SpriteBatch sb, Vector2 offset)
        {
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, Position - offset, null, Color.White, 0f, origin, (float)Diametre / Texture.Width, SpriteEffects.None, 0f);
        }
        
        public override void AddTemporaryForce(Vector2 force)
            => base.AddTemporaryForce(force / Mass);
    }
}