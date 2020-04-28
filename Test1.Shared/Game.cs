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
        private sealed class TestObject : Accelerable
        {
            public Texture2D Texture { get; set; }

            public void Draw(SpriteBatch sb)
            {
                sb.Draw(Texture, Position, Color.White);
            }
        }

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
            _obj = new TestObject()
            {
                Friction = .99f
            };
            _obj.AddConstantForce(Vector2.UnitY * .3f);
            _obj.AddTemporaryForce(Vector2.UnitX * 15f);

            UsePlatformSpecificFileSource();
            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            contentManager = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            texture = contentManager.Load<Texture2D>("desktop_uv256");
            _draw = Using.Create(spriteBatch.Begin, spriteBatch.End);
            _obj.Texture = texture;

            base.OnLoadingContent();
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            var ups = 1 / time.ElapsedTime.TotalSeconds;
            var upsRatio = TargetElapsedTime / time.ElapsedTime;
            if (Ultraviolet.GetInput().GetActions().ExitApplication.IsPressed())
            {
                Exit();
            }
            _obj.Update();
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
                _obj.Draw(spriteBatch);
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
        private TestObject _obj = null!;

        private Using.IDisposable _draw = null!;
    }
}
