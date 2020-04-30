using System;
using Ultraviolet;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public class Ship : Mover
    {
        public Ship()
            : base(50)
        { }

        private float _rotation;
        public float Rotation
        {
            get => _rotation;
            set
            {
                var twoPi = MathF.PI * 2;
                _rotation = ((value % twoPi) + twoPi) % twoPi;
            }
        }

        public void AddTemporaryForce(Vector2 force, bool local)
            => base.AddTemporaryForce((local ? Rotate(force, Rotation) : force));

        private static Vector2 Rotate(in Vector2 vector, float radians)
        {
            var cos = MathF.Cos(radians);
            var sin = MathF.Sin(radians);
            return new Vector2(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
        }

        public void Draw(SpriteBatch sb, Vector2 offset)
        {
            var radius = 6f;
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, Position - offset, null, Color.Yellow * .75f, 0f, origin, (radius+radius) / Texture.Width, SpriteEffects.None, 0f);
            // sb.Draw(Globals.Pixel, Position - offset + Vector2.Normalize(Acceleration) * radius / 2, null, Color.Red, 0f, Vector2.One/2, 2f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, Position - offset + Rotate(Vector2.UnitY, Rotation) * (radius + .1f), null, Color.Green, 0f, Vector2.One/2, 3f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, Position - offset + Vector2.Normalize(Velocity) * radius / 2, null, Color.Blue, 0f, Vector2.One/2, 2f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, Position - offset, Color.Black);
        }
    }
}