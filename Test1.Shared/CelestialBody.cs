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
                Mass = (int)(Volume * Density);
            }
        }

        public float Density { get; private set; }

        // 2D World
        // public int Volume
        // {
        //     get => (int)(System.MathF.PI * Radius * Radius);
        //     set => Radius = (int)System.MathF.Sqrt(value / System.MathF.PI);
        // }

        // 3D World
        public int Volume
        {
            get => (int)( 4 /3f * System.MathF.PI * Radius * Radius * Radius);
            set => Radius = (int)System.MathF.Cbrt(value / System.MathF.PI / (4 / 3f));
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
                    if (b.Mass > a.Mass || b is BlackHole)
                        (a, b) = (b, a);

                    a.Merge(b);
                    other.AddTemporaryForce(force);
                }
                else if (other is Ship ship && vector.Length() <= Radius)
                {
                    var relativeVelocity = ship.Velocity - Velocity;
                    ship.Velocity = Velocity;
                }
                else
                    other.AddTemporaryForce(force);
            }
        }

        private void Merge(CelestialBody other)
        {
            var initialMass = Mass;
            var volume = Volume + other.Volume;
            var mass = Mass + other.Mass;
            Density = mass / volume;
            Volume = volume;
            Mass = mass;

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

        public void Draw(SpriteBatch sb, Vector2 offset, Movable? reference, int zoom = 1)
        {
            var isZommed = zoom != 1;
            var diametre = Diametre * zoom;
            var radius = diametre / 2;
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, isZommed ? offset : Position - offset, null, (Selected && !isZommed ? Color.LawnGreen : Color) * .75f, 0f, origin, (float)diametre / Texture.Width, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Acceleration) * radius / 2, null, Color.Red, 0f, Vector2.One/2, zoom * 2f, SpriteEffects.None, 0f);
            if(reference is Movable body && !object.ReferenceEquals(this, body))
                sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Velocity - body.Velocity) * radius / 4, null, Color.Blue, 0f, Vector2.One/2, zoom * 2f, SpriteEffects.None, 0f);
            else
                sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Velocity) * radius / 4, null, Color.Blue, 0f, Vector2.One/2, zoom * 2f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, isZommed ? offset : Position - offset, Color.Black);
        }

        protected Color Color { get; set; } = Color.White;
    }

    public class BlackHole : CelestialBody
    {
        public BlackHole(int radius, float density)
            : base(radius, density * 100)
        {
            Color = Color.DarkSlateGray;
        }
    }
}