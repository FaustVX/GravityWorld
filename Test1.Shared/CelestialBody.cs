using Ultraviolet;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public class CelestialBody : Mover
    {
        public CelestialBody(int radius, float density)
            : base(0)
        {
            Density = density;
            Radius = radius;
        }

        private int _radius;
        public int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                Mass = (int)(Surface * Density);
            }
        }

        public float Density { get; private set; }

        public int Surface
        {
            get => (int)(System.MathF.PI * Radius * Radius);
            set => Radius = (int)System.MathF.Sqrt(value / System.MathF.PI);
        }
        
        public int Diametre
        {
            get => Radius * 2;
            set => Radius = value / 2;
        }

        public bool Selected { get; set; }

        public void Attract(Mover other)
        {
            if (Mass is 0 || other.Mass is 0)
                return;

            if(CalculateGravity(other.Position, other.Mass, out var vector, out var force))
            {
                if (other is CelestialBody o && vector.Length() <= (o.Radius + Radius) * 0.65f)
                {
                    var normal = Vector2.Normalize(vector);
                    var reflected = Vector2.Reflect(o.Velocity, normal);

                    var (a, b) = (this, o);
                    if (b.Mass > a.Mass)
                        (a, b) = (b, a);

                    a.Merge(b);
                }

                other.AddTemporaryForce(force);
            }
        }

        private void Merge(CelestialBody other)
        {
            Mass += other.Mass;
            Surface += other.Surface;
            Density = Mass / Surface;
            AddTemporaryForce(other.Velocity);
            other.Radius = 0;
            Selected |= other.Selected;
        }

        public bool CalculateGravity(Vector2 position, float mass, out Vector2 vector, out Vector2 force)
        {
            vector = Position - position;
            var lengthSquared = vector.LengthSquared();
            force = Vector2.Normalize(vector) * (Globals.G * (mass * Mass) / lengthSquared);
            return true;
        }

        public void Draw(SpriteBatch sb, Vector2 offset, Movable? reference)
        {
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, Position - offset, null, (Selected ? Color.LawnGreen : Color.White) * .75f, 0f, origin, (float)Diametre / Texture.Width, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, Position - offset + Vector2.Normalize(Acceleration) * Radius / 2, null, Color.Red, 0f, Vector2.One/2, 2f, SpriteEffects.None, 0f);
            if(reference is Movable body && !object.ReferenceEquals(this, body))
                sb.Draw(Globals.Pixel, Position - offset + Vector2.Normalize(Velocity - body.Velocity) * Radius / 4, null, Color.Blue, 0f, Vector2.One/2, 2f, SpriteEffects.None, 0f);
            else
                sb.Draw(Globals.Pixel, Position - offset + Vector2.Normalize(Velocity) * Radius / 4, null, Color.Blue, 0f, Vector2.One/2, 2f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, Position - offset, Color.Black);
        }
    }
}