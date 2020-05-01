using System;
using Ultraviolet;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public class Ship : Mover
    {
        public Ship()
            : base(50)
        {
            AngleFriction = .85f;
        }

        public void AddTemporaryForce(Vector2 force, bool local)
            => base.AddTemporaryForce((local ? Rotate(force, Rotation) : force));

        private static Vector2 Rotate(in Vector2 vector, float radians)
        {
            var cos = MathF.Cos(radians);
            var sin = MathF.Sin(radians);
            return new Vector2(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
        }

        public void Draw(SpriteBatch sb, Vector2 offset, Movable? reference, int zoom = 1)
        {
            var isZommed = zoom != 1;
            var radius = 6f * zoom;
            var origin = new Vector2(Texture.Width, Texture.Height) / 2;
            sb.Draw(Texture, isZommed ? offset : Position - offset, null, Color.Yellow * .75f, 0f, origin, (radius + radius) / Texture.Width, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Rotate(-Vector2.UnitY, Rotation) * (radius + .1f), null, Color.Green, 0f, Vector2.One/2, zoom * 3f, SpriteEffects.None, 0f);
            if(reference is Movable body)
                sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Velocity - body.Velocity) * radius / 2, null, Color.Blue, 0f, Vector2.One/2, zoom * 2f, SpriteEffects.None, 0f);
            else
                sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Velocity) * radius / 2, null, Color.Blue, 0f, Vector2.One/2, zoom * 2f, SpriteEffects.None, 0f);
            if (isZommed)
                sb.Draw(Globals.Pixel, (isZommed ? offset : Position - offset) + Vector2.Normalize(Acceleration) * radius / 2, null, Color.Red, 45, Vector2.One/2, 4f, SpriteEffects.None, 0f);
            sb.Draw(Globals.Pixel, isZommed ? offset : Position - offset, Color.Black);
        }
    }
}