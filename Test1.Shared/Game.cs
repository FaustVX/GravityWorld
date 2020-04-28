using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            texture = contentManager.Load<Texture2D>("desktop_uv256");
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
            var actions = Ultraviolet.GetInput().GetGlobalActions();
            if (actions.ExitApplication.IsPressed())
            {
                if (SelectedMover is null)
                    Exit();
                else
                    SelectedMover = null;
            }
            else if (actions.RestartApplication.IsPressed())
            {
                Reset();
            }
            else if (actions.NextPlanet.IsPressed())
            {
                var old = _movers.IndexOf(SelectedMover!);
                SelectedMover = _movers.Count <= 0 ? null! : _movers[(old + 1) % _movers.Count];
            }
            else if (actions.DeletePlanet.IsPressed())
            {
                _movers.Remove(SelectedMover!);
                SelectedMover = null!;
            }

            var offsetSpeed = 5;
            if(actions.Up.IsDown())
                _offset -= Vector2.UnitY * offsetSpeed;
            if(actions.Down.IsDown())
                _offset += Vector2.UnitY * offsetSpeed;
            if(actions.Left.IsDown())
                _offset -= Vector2.UnitX * offsetSpeed;
            if(actions.Right.IsDown())
                _offset += Vector2.UnitX * offsetSpeed;
            
            foreach (var mover in _movers)
            {
                foreach (var other in _movers)
                    if(!object.ReferenceEquals(mover, other))
                        mover.Attract(other);
            }
            _movers.RemoveAll(m => m.Radius is 0);

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
                var offset = SelectedMover is Mover m ? _offset + m.Position - new Vector2(window.ClientSize.Width, window.ClientSize.Height) / 2 : _offset;
#if DEBUG
                var size = 5;
                var half = size / 2f;
                var width = (window.ClientSize.Width / size) + 1;
                var height = (window.ClientSize.Height / size) + 1;

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        var position = new Vector2(x * size + half, y * size + half) + offset;

                        var force = Vector2.Zero;
                        foreach (var mover in _movers)
                        {
                            mover.CalculateGravity(position, 1, out _, out var force2);
                            force += force2;
                        }
                        var magnitude = force.Length();

                        spriteBatch.Draw(pixel, new RectangleF(x * size, y * size, size, size), Color.FromRgba((uint)(magnitude * uint.MaxValue)));
                    }
#endif
                foreach (var mover in _movers)
                    mover.Draw(spriteBatch, offset);
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

        private void Reset()
        {
            var rng = new Random();
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
            var border = 100;
            var velocity = 75;
            var velRatio = 1000;

            _movers = new List<Mover>(rng.Next(5, 25));
            for (int i = 0; i < _movers.Capacity; i++)
            {
                var mover = new Mover(rng.Next(10, 30))
                {
                    Position = new Vector2(rng.Next(border, window.Width - border), rng.Next(border, window.Height - border)),
                    // Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / velRatio,
                    Texture = texture
                };
                _movers.Add(mover);
            }
            _offset = Vector2.Zero;
        }

        private ContentManager contentManager = null!;
        private SpriteBatch spriteBatch = null!;
        private Texture2D texture = null!, pixel = null!;
        private List<Mover> _movers = null!;
        private Mover? SelectedMover
        {
            get => _movers.FirstOrDefault(m => m.Selected);
            set
            {
                foreach (var mover in _movers)
                    mover.Selected = false;
                _offset = Vector2.Zero;
                if (value is {})
                    value.Selected = true;
            }
        }

        private Using.IDisposable _draw = null!;

        private Vector2 _offset = Vector2.Zero;
    }
}
