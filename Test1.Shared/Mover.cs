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

        public bool Selected { get; set; }

        public Texture2D Texture { get; set; } = null!;

        public virtual void Attract(Mover other)
        {
            if(Mass is 0 || other.Mass is 0)
                return;
            
            CalculateGravity(other.Position, other.Mass, out var vector, out var force);
            if (vector.Length() <= (other.Radius + Radius) * 0.8f)
            {
                var normal = Vector2.Normalize(vector);
                var reflected = Vector2.Reflect(other.Velocity, normal);

                var (a, b) = (this, other);
                if(b is Attractor || b.Mass > a.Mass)
                    (a, b) = (b, a);

                a.Mass += b.Mass;
                a.AddTemporaryForce(b.Velocity);
                b.Radius = 0;
                a.Selected |= b.Selected;
            }

            other.AddTemporaryForce(force);
        }

        public virtual void CalculateGravity(Vector2 position, int mass, out Vector2 vector, out Vector2 force)
        {
            var G = .005f;
            vector = Position - position;
            var lengthSquared = vector.LengthSquared();
            force = Vector2.Normalize(vector) * (G * (mass * Mass) / lengthSquared);
        }

        public virtual void Draw(SpriteBatch sb, Vector2 offset)
        {
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, Position - offset, null, Selected ? Color.LawnGreen * .75f : Color.White, 0f, origin, (float)Diametre / Texture.Width, SpriteEffects.None, 0f);
        }
        
        public override void AddTemporaryForce(Vector2 force)
            => base.AddTemporaryForce(force / Mass);
    }
}