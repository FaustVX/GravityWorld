using Ultraviolet;
using Ultraviolet.Input;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public sealed class Attractor : Positionable
    {
        public Attractor(MouseDevice mouse, int radius)
        {
            Radius = radius;
            Mouse = mouse;
        }

        public MouseDevice Mouse { get; }
        public Texture2D Texture { get; set; } = null!;

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
            set => Radius = Diametre / 2;
        }

        private int _mass;
        public int Mass
        {
            get => _mass;
            set
            {
                _mass = value;
                _radius = (int)(System.MathF.Sqrt(Mass) / System.MathF.PI);
            }
        }

        public override Vector2 Position
        {
            get => base.Position = new Vector2(Mouse.Position.X, Mouse.Position.Y);
            set => base.Position = Position;
        }

        public void Attract(Mover mover)
        {
            var G = .025f;
            var vector = Position - mover.Position;
            if(vector.Length() <= mover.Radius + Radius)
            {
                var normal = Vector2.Normalize(vector);
                var reflected = Vector2.Reflect(mover.Velocity, normal);
                mover.Velocity = reflected;
            }
            var lengthSquared = vector.LengthSquared();
            var force = Vector2.Normalize(vector) * (G * (mover.Mass * Mass) / lengthSquared);

            mover.AddTemporaryForce(force);
        }

        public void Draw(SpriteBatch sb)
        {
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, Position, null, Color.White, 0f, origin, (float)Diametre / Texture.Width, SpriteEffects.None, 0f);
        }
    }
}