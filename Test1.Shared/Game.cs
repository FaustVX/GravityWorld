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
            _ship = new Ship();
            Reset();

            UsePlatformSpecificFileSource();
            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            contentManager = ContentManager.Create("Content");
            spriteBatch = SpriteBatch.Create();
            Globals.Pixel = Texture2D.CreateTexture(1, 1);
            Globals.Pixel.SetData(new[] { Color.White });
            texture = _ship.Texture = contentManager.Load<Texture2D>("desktop_uv256");
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
            _movers.Cast<Mover>().Prepend(_ship).AsParallel().AsUnordered().ForAll(m => m.ResetAccelerations());
            
            var globalActions = Ultraviolet.GetInput().GetGlobalActions();
            if (globalActions.RestartApplication.IsPressed())
            {
                Reset();
            }
            else if (globalActions.NextPlanet.IsPressed())
            {
                var old = _movers.IndexOf(SelectedMover!);
                SelectedMover = _movers.Count <= 0 ? null! : _movers[(old + 1) % _movers.Count];
            }
            else if (globalActions.DeselectPlanet.IsPressed())
            {
                SelectedMover = null;
            }
            _paused ^= globalActions.PlaySimulation.IsPressed();
            var runThisFrame = !_paused || globalActions.StepSimulation.IsPressed(ignoreRepeats: false);

            if(_inShip)
            {
                var actions = Ultraviolet.GetInput().GetShipActions();
                if (actions.ExitShip.IsPressed())
                {
                    _inShip = false;
                }
                
                var isShiftDown = Ultraviolet.GetInput().GetKeyboard().IsShiftDown;
                var vector = Vector2.UnitY * (isShiftDown ? 1500f : 400f);
                var rotation = isShiftDown ? 200f : 70f;

                if (actions.Forward.IsDown())
                    _ship.AddTemporaryForce(vector, local: true);
                if(actions.Backward.IsDown())
                    _ship.AddTemporaryForce(vector / 3, local: true);
                if (actions.Left.IsDown())
                    _ship.AngleAcceleration -= rotation;
                if(actions.Right.IsDown())
                    _ship.AngleAcceleration += rotation;
            }
            else
            {
                var actions = Ultraviolet.GetInput().GetNotInShipActions();
                if (actions.EnterShip.IsPressed())
                {
                    _inShip = true;
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
            }

            if (runThisFrame)
            {
                foreach (var mover in _movers)
                    _movers.Cast<Mover>().Prepend(_ship).AsParallel().AsUnordered().Where(other => ! object.ReferenceEquals(mover, other)).ForAll(mover.Attract);

                _movers.RemoveAll(m => m.Radius is 0);

                _movers.Cast<Mover>().Prepend(_ship).AsParallel().AsUnordered().ForAll(m => m.Update(time));
            }
            
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
                var offset = _ship.Position - new Vector2(window.ClientSize.Width, window.ClientSize.Height) / 2;
                if (_inShip)
                {
                    window.Caption = $"{(int)fps}fps -- {_ship.Mass}kg - {_ship.Velocity.Length():0.00000}m/s - {_ship.Acceleration.Length():0.00000}m/s/s - {_ship.Rotation:0.00000}rad - {_ship.AngleVelocity:0.00000}rad/s - {_ship.AngleAcceleration:0.00000}rad/s/s";
                    if(SelectedMover is CelestialBody body)
                    {
                        window.Caption += $" -- {(_ship.Position - body.Position).Length():0.00}m - {(_ship.Velocity - body.Velocity).Length():0.00}m/s";
                    }
                }
                else
                {
                    window.Caption = SelectedMover is CelestialBody m1 ? $"{(int)fps}fps -- {m1.Mass}kg - {m1.Density:0.00000}kg/m3 - {m1.Volume}m3 - {m1.Diametre}m - {m1.Velocity.Length():0.00000}m/s - {m1.Acceleration.Length():0.00000}m/s/s" : $"{(int)fps}fps";
                    offset = SelectedMover is CelestialBody m ? _offset + m.Position - new Vector2(window.ClientSize.Width, window.ClientSize.Height) / 2 : _offset;
                }
                
#if DEBUG
                if (Ultraviolet.GetInput().GetGlobalActions().ShowGravity.IsUp())
#else
                if (Ultraviolet.GetInput().GetGlobalActions().ShowGravity.IsDown())
#endif
                {
                    var size = 15;
                    var half = size / 2f;
                    var width = (window.ClientSize.Width / size) + 1;
                    var height = (window.ClientSize.Height / size) + 1;
                    var mouse = Ultraviolet.GetInput().GetMouse().Position;

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
#if DEBUG
                            var rect = new RectangleF(position.X, position.Y, size, size);
                            if(rect.Contains(mouse.X + offset.X, mouse.Y + offset.Y))
                                Ultraviolet.GetPlatform().Windows.GetPrimary().Caption += $" -- pointed gravity: {magnitude:0.00000}m/s/s";
#endif

                            magnitude *= Globals.G / 2;
                            var color = magnitude <= 1 ? Color.Green.Interpolate(Color.Blue, EasingFunction(magnitude, 3)) : Color.Red.Interpolate(Color.Blue, 1 / (magnitude));

                            spriteBatch.Draw(Globals.Pixel, new RectangleF(x * size, y * size, size, size), color);

                            static float EasingFunction(float magnitude, byte level)
                                => MathF.Sqrt(1 - MathF.Pow(magnitude - 1, level * 5 * .4f));
                        }
                }
                foreach (var mover in _movers.Where(mover => new RectangleF(Point2F.Zero, window.ClientSize).Contains(mover.Position - offset)))
                    mover.Draw(spriteBatch, offset, _inShip ? (Movable)_ship : SelectedMover);
                _ship.Draw(spriteBatch, _inShip ? _ship.Position - new Vector2(window.ClientSize.Width, window.ClientSize.Height) / 2 : offset, SelectedMover);
                _ship.Draw(spriteBatch, new Vector2(100, 50), SelectedMover, zoom: 4);
                SelectedMover?.Draw(spriteBatch, new Vector2(100, 250), _inShip ? _ship : null, zoom: 3);
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
#if DEBUG
            var rng = new Random(0);
#else
            var rng = new Random();
#endif
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
            var border = -10;
            var velocity = 800;
            var velRatio = 100f;
            
            _movers = new List<CelestialBody>(200);
            for (int i = 1; i < _movers.Capacity; i++)
            {
                var mover = new CelestialBody(rng.Next(5, 15), (float)rng.NextDouble() * 25)
                {
                    Position = new Vector2(rng.Next(border, window.Width - border), rng.Next(border, window.Height - border)),
                    Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / velRatio,
                    Texture = texture
                };
                _movers.Add(mover);
            }
            var hole = new BlackHole(rng.Next(5, 15), (float)rng.NextDouble() * 25)
            {
                Position = new Vector2(rng.Next(border, window.Width - border), rng.Next(border, window.Height - border)),
                // Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / (velRatio / 10),
                Texture = texture
            };
            _movers.Add(hole);
            _movers.Reverse();
            _offset = Vector2.Zero;

            _ship.Position = new Vector2(window.Width, window.Height) / 2;
            _ship.ResetAccelerations();
            _ship.Rotation = 0;
            _ship.AngleVelocity = 0;
            _ship.Velocity = Vector2.Zero;
        }

        private ContentManager contentManager = null!;
        private SpriteBatch spriteBatch = null!;
        private Texture2D texture = null!;
        private Ship _ship = null!;
        private List<CelestialBody> _movers = null!;
        private CelestialBody? SelectedMover
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
#if DEBUG
        private bool _paused = true, _inShip = false;
#else
        private bool _paused = false, _inShip = true;
#endif
    }
}