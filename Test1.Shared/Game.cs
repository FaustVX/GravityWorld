using System;
using System.Diagnostics;
using Ultraviolet;
using Ultraviolet.BASS;
using Ultraviolet.Content;
using Ultraviolet.FreeType2;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.OpenGL;
using static test1.Input.MyInputs;

namespace test1
{
    public partial class Game : UltravioletApplication
    {
        public Game()
            : base("FaustVX", "My Test1")
        { }

        protected override UltravioletContext OnCreatingUltravioletContext()
        {
            var configuration = new OpenGLUltravioletConfiguration();
            configuration.Plugins.Add(new BASSAudioPlugin());
            configuration.Plugins.Add(new FreeTypeFontPlugin());

#if DEBUG
            configuration.Debug = true;
            configuration.DebugLevels = DebugLevels.Error | DebugLevels.Warning;
            configuration.DebugCallback = (uv, level, message) =>
            {
                System.Diagnostics.Debug.WriteLine(message);
            };
#endif

            return new OpenGLUltravioletContext(this, configuration);
        }

        protected override void OnInitialized()
        {
            Ultraviolet.GetInput().GetMouse().SetIsRelativeModeEnabled(true);
            _attractor = new Attractor(Ultraviolet.GetInput().GetMouse(), 50);
            Reset();

            UsePlatformSpecificFileSource();
            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            contentManager = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            pixel = Texture2D.CreateTexture(1, 1);
            pixel.SetData(new[] { Color.White });
            _attractor.Texture = texture = contentManager.Load<Texture2D>("desktop_uv256");
            foreach (var mover in _movers)
                mover.Texture = texture;
            _draw = Using.Create(spriteBatch.Begin, spriteBatch.End);
            base.OnLoadingContent();
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            var ups = 1 / time.ElapsedTime.TotalSeconds;
            var upsRatio = TargetElapsedTime / time.ElapsedTime;
            Debug.WriteLine($"UPS: {ups}");
            Ultraviolet.GetInput().GetMouse().WarpToPrimaryWindowCenter();
            var actions = Ultraviolet.GetInput().GetGlobalActions();
            if (actions.ExitApplication.IsPressed())
            {
                Exit();
            } else if (actions.RestartApplication.IsPressed())
            {
                Reset(Ultraviolet.GetInput().GetMouse().IsShiftDown);
            }

            var offsetSpeed = 5;
            if(actions.Up.IsPressed(ignoreRepeats: false))
                _offset -= Vector2.UnitY * offsetSpeed;
            if(actions.Down.IsPressed(ignoreRepeats: false))
                _offset += Vector2.UnitY * offsetSpeed;
            if(actions.Left.IsPressed(ignoreRepeats: false))
                _offset -= Vector2.UnitX * offsetSpeed;
            if(actions.Right.IsPressed(ignoreRepeats: false))
                _offset += Vector2.UnitX * offsetSpeed;
            
            foreach (var mover in _movers)
            {
                _attractor.Attract(mover);
                foreach (var other in _movers)
                    if(!object.ReferenceEquals(mover, other))
                        mover.Attract(other);
            }

            _attractor.Update();
            foreach (var mover in _movers)
            mover.Update();
            base.OnUpdating(time);
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            var fps = 1 / time.ElapsedTime.TotalSeconds;
            Debug.WriteLine($"FPS: {fps}");
            var fpsRatio = TargetElapsedTime / time.ElapsedTime;
            var window = Ultraviolet.GetPlatform().Windows.GetCurrent();

            using (_draw.Start())
            {
                var size = 8;
                var half = size / 2f;
                for (int x = 0; x < window.ClientSize.Width / size; x++)
                {
                    for (int y = 0; y < window.ClientSize.Height / size; y++)
                    {
                        var position = new Vector2(x * size + half, y * size + half) + _offset;

                        _attractor.CalculateGravity(position, 1, out _, out var force);
                        foreach (var mover in _movers)
                        {
                            mover.CalculateGravity(position, 1, out _, out var force2);
                            force += force2;
                        }
                        var magnitude = force.Length();

                        spriteBatch.Draw(pixel, new RectangleF(x * size, y * size, size, size), Color.FromRgba((uint)(magnitude * uint.MaxValue)));
                    }
                }
                foreach (var mover in _movers)
                    mover.Draw(spriteBatch, _offset);
                _attractor.Draw(spriteBatch, _offset);
            }

            base.OnDrawing(time);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                contentManager?.Dispose();
                spriteBatch?.Dispose();
                texture?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Reset(bool resetMousePosition = true)
        {
            var rng = new Random();
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
            var border = 100;
            var velocity = 75;
            var velRatio = 1000;

            _movers = new Mover[rng.Next(1, 10)];
            for (int i = 0; i < _movers.Length; i++)
            {
                _movers[i] = new Mover(rng.Next(10, 30))
                {
                    Position = new Vector2(rng.Next(border, window.Width - border), rng.Next(border, window.Height - border)),
                    Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / velRatio,
                    Texture = texture
                };
            }
            if(resetMousePosition)
            {
                _attractor.Position = new Vector2(window.Width / 2, window.Height / 2);
                _attractor.Attractable = true;
            }
            _offset = Vector2.Zero;
        }

        private ContentManager contentManager = null!;
        private SpriteBatch spriteBatch = null!;
        private Texture2D texture = null!, pixel = null!;
        private Attractor _attractor = null!;
        private Mover[] _movers = null!;

        private Using.IDisposable _draw = null!;

        private Vector2 _offset = Vector2.Zero;
    }
}
