using Ultraviolet;
using Ultraviolet.Input;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;

namespace test1
{
    public sealed class Attractor : Mover
    {
        public Attractor(MouseDevice mouse, int radius)
            : base(radius)
        {
            Mouse = mouse;
            Mouse.SetIsRelativeModeEnabled(Attractable);
            Mouse.Moved += (w, d, x, y, dx, dy) =>
            {
                var displacement = Attractable ? new Vector2(dx, dy) : Vector2.Zero;
                if (Selected)
                    displacement *= -1;
                
                Position += displacement;
            };
            Selected = true;
        }

        public MouseDevice Mouse { get; }
        public bool Attractable { get; set; } = true;

        public override void Attract(Mover mover)
        {
            if (!Attractable)
                return;

            base.Attract(mover);
        }

        public override void CalculateGravity(Vector2 position, int mass, out Vector2 vector, out Vector2 force)
        {
            if (!Attractable)
            {
                vector = force = Vector2.Zero;
                return;
            }
            base.CalculateGravity(position, mass, out vector, out force);
        }

        public override void Draw(SpriteBatch sb, Vector2 offset)
        {
            if(!Attractable)
                return;
            
            base.Draw(sb, offset);
        }

        public override void Update()
        {
            if(Mouse.IsButtonPressed(MouseButton.Right))
            {
                Attractable ^= true;
                Mouse.SetIsRelativeModeEnabled(Attractable);
            }
            if(Attractable)
                Mouse.WarpToPrimaryWindowCenter();
        }
    }
}