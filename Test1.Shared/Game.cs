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
            UsePlatformSpecificFileSource();
            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            contentManager = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            texture = contentManager.Load<Texture2D>("desktop_uv256");
            _draw = Using.Create(spriteBatch.Begin, spriteBatch.End);

            base.OnLoadingContent();
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            if (Ultraviolet.GetInput().GetActions().ExitApplication.IsPressed())
            {
                Exit();
            }
            base.OnUpdating(time);
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            var fps = 1 / time.ElapsedTime.TotalSeconds;
            var fpsRatio = TargetElapsedTime / time.ElapsedTime;
            var window = Ultraviolet.GetPlatform().Windows.GetCurrent();
            var mouse = Ultraviolet.GetInput().GetMouse().Position;
            var mouseRatio = new Vector2(mouse.X, mouse.Y) / new Vector2(window.ClientSize.Width, window.ClientSize.Height);
            var position = new Vector2(window.ClientSize.Width, window.ClientSize.Height) * mouseRatio;
            var origin = new Vector2(texture.Width, texture.Height) * mouseRatio;

            using (_draw.Start())
            {
                spriteBatch.Draw(texture, new RectangleF(Point2F.Zero, window.ClientSize), Color.White * .25f);
                spriteBatch.Draw(texture, position, null, Color.White * .8f, 0f, origin, 1f, SpriteEffects.None, 0f);
            }

            base.OnDrawing(time);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                contentManager?.Dispose();
                spriteBatch?.Dispose();
            }
            base.Dispose(disposing);
        }

        private ContentManager contentManager;
        private SpriteBatch spriteBatch;
        private Texture2D texture;

        private Using.IDisposable _draw;
    }
}
