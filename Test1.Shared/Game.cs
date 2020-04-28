using System;
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
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
            _attractor = new Attractor(Ultraviolet.GetInput().GetMouse(), 50);
            _mover = new Mover(15)
            {
                Position = new Vector2(window.Width, window.Height) / 2
            };

            UsePlatformSpecificFileSource();
            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            contentManager = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            _mover.Texture = _attractor.Texture = texture = contentManager.Load<Texture2D>("desktop_uv256");
            _draw = Using.Create(spriteBatch.Begin, spriteBatch.End);
            base.OnLoadingContent();
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            var ups = 1 / time.ElapsedTime.TotalSeconds;
            var upsRatio = TargetElapsedTime / time.ElapsedTime;
            var actions = Ultraviolet.GetInput().GetActions();
            if (actions.ExitApplication.IsPressed())
            {
                Exit();
            } else if (actions.RestartApplication.IsPressed())
            {
                var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
                _mover = new Mover(15)
                {
                    Position = new Vector2(window.Width, window.Height) / 2,
                    Texture = texture
                };
            }
            _attractor.Attract(_mover);
            _mover.Update();
            base.OnUpdating(time);
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            var fps = 1 / time.ElapsedTime.TotalSeconds;
            var fpsRatio = TargetElapsedTime / time.ElapsedTime;
            var window = Ultraviolet.GetPlatform().Windows.GetCurrent();

            using (_draw.Start())
            {
                spriteBatch.Draw(texture, new RectangleF(Point2F.Zero, window.ClientSize), new Color(1f, 1f, 1f, 0.25f));
                _mover.Draw(spriteBatch);
                _attractor.Draw(spriteBatch);
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

        private ContentManager contentManager = null!;
        private SpriteBatch spriteBatch = null!;
        private Texture2D texture = null!;
        private Attractor _attractor = null!;
        private Mover _mover = null!;

        private Using.IDisposable _draw = null!;
    }
}
