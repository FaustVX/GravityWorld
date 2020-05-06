using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ultraviolet;
using Ultraviolet.BASS;
using Ultraviolet.Content;
using Ultraviolet.FreeType2;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.OpenGL;
using static test1.Input.MyInputs;
using System.Threading;

namespace test1
{
    public partial class Game : UltravioletApplication
    {
        public Game()
            : base("FaustVX", "My Test1")
        {
            _gravityThread = new Thread(CalculateGravityField);

            void CalculateGravityField()
            {
                var size = 8;
                var window = Ultraviolet.GetPlatform().Windows.GetPrimary();

                while (true)
                    if(_ship is Ship ship && _movers?.ToArray() is CelestialBody[] movers && SelectedMover is var selectedMover)
                    {
                        var windowSize = new Vector2(window.ClientSize.Width, window.ClientSize.Height);
                        var centerWindow = windowSize / 2;
                        
                        var width = (window.ClientSize.Width / size) + 1;
                        var height = (window.ClientSize.Height / size) + 1;
                        var field = new (Rectangle rect, Color color)[width, height];
                        var half = size / 2f;
#if DEBUG
                        var mouse = Ultraviolet.GetInput().GetMouse().Position;
#endif
                        var offset = TotalOffset - centerWindow;

                        for (var x = 0; x < width; x++)
                            for (var y = 0; y < height; y++)
                            {
                                var position = new Vector2(x * size + half, y * size + half) + offset;

                                var force = Vector2.Zero;
                                for (int i = 0; i < movers.Length; i++)
                                {
                                    var mover = movers[i];
                                    if ((mover.Position - position).LengthSquared() >= mover.CalculateGravityRadiusSquared(1, Globals.MinimumGravityForCalculation))
                                        continue;
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

                                field[x, y] = (new Rectangle(x * size, y * size, size, size), color);

                                static float EasingFunction(float magnitude, byte level)
                                    => MathF.Sqrt(1 - MathF.Pow(magnitude - 1, level * 5 * .4f));
                            }
                        _gravityField = field;
                    }
            }
        }

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
            _ups = 1 / time.ElapsedTime.TotalSeconds;
            // var upsRatio = TargetElapsedTime / time.ElapsedTime;
            Debug.WriteLine($"UPS: {_ups}");
            _ship.ResetAccelerations();
            for (int i = 0; i < _movers.Count; i++)
                _movers[i].ResetAccelerations();
            
            var keyboardDevice = Ultraviolet.GetInput().GetKeyboard();
            var mouseDevice = Ultraviolet.GetInput().GetMouse();
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary();

            if (mouseDevice.IsButtonClicked(global::Ultraviolet.Input.MouseButton.Left))
            {
                var mousePos = TotalOffset + new Vector2(mouseDevice.X, mouseDevice.Y) - new Vector2(window.ClientSize.Width, window.ClientSize.Height) / 2;
                for (var i = 0; i < _movers.Count; i++)
                {
                    var mover = _movers[i];
                    if ((mover.Position - mousePos).LengthSquared() <= mover.Radius * mover.Radius)
                    {
                        SelectedMover = mover;
                        break;
                    }
                }
            }
            
            var globalActions = Ultraviolet.GetInput().GetGlobalActions();
            if (globalActions.RestartApplication)
            {
                _paused = true;
                Reset();
            }
            else if (globalActions.NextPlanet)
            {
                var old = _movers.IndexOf(SelectedMover!);
                var count = _movers.Count;
                var nextIndex = old + (keyboardDevice.IsShiftDown ? -1 : 1);
                SelectedMover = count <= 0 ? null! : _movers[((nextIndex % count) + count) % count];
            }
            else if (globalActions.DeselectPlanet)
            {
                SelectedMover = null;
            }
            else if (globalActions.TimeRatio1)
            {
                Globals.TimeRatio = 1;
            }
            else if (globalActions.TimeRatio2)
            {
                Globals.TimeRatio = 2;
            }
            else if (globalActions.TimeRatio3)
            {
                Globals.TimeRatio = 4;
            }
            else if (globalActions.TimeRatio4)
            {
                Globals.TimeRatio = 8;
            }
            TogglePause(globalActions.PlaySimulation);
            var runThisFrame = !_paused || globalActions.StepSimulation;
            
            var shipActions = Ultraviolet.GetInput().GetShipActions();
            var isShiftDown = keyboardDevice.IsShiftDown;

            if (_inShip)
            {
                if (shipActions.ExitShip)
                {
                    _inShip = false;
                }
            }
            else
            {
                var actions = Ultraviolet.GetInput().GetNotInShipActions();
                if (actions.EnterShip)
                {
                    _inShip = true;
                }
                else if (actions.DeletePlanet)
                {
                    _movers.Remove(SelectedMover!);
                    SelectedMover = null!;
                }

                var offsetSpeed = 5;
                if(actions.Up)
                    _offset -= Vector2.UnitY * offsetSpeed;
                if(actions.Down)
                    _offset += Vector2.UnitY * offsetSpeed;
                if(actions.Left)
                    _offset -= Vector2.UnitX * offsetSpeed;
                if(actions.Right)
                    _offset += Vector2.UnitX * offsetSpeed;
            }
            var vector = Vector2.Zero;
            var angleAcceleration = 0f;

            if (shipActions.Forward)
                vector += Vector2.UnitY * (isShiftDown ? 1500f : 400f);
            if(shipActions.Backward)
                vector += -Vector2.UnitY * (isShiftDown ? 1500f : 400f) / 3;
            if (shipActions.Left)
                angleAcceleration -= 70f;
            if(shipActions.Right)
                angleAcceleration += 70f;

            var timeRatio = Globals.TimeRatio;
            while (runThisFrame && timeRatio-- > 0)
            {
                if(_inShip)
                {
                    _ship.AddTemporaryForce(vector, local: true);
                    _ship.AngleAcceleration += angleAcceleration;
                }

                for (var i = 0; i < _movers.Count; i++)
                {
                    var mover = _movers[i];
                    mover.Attract(_ship);
                    for (var j = 0; j < _movers.Count; j++)
                    {
                        var other = _movers[j];
                        if(object.ReferenceEquals(mover, other))
                            continue;
                        if((other.Position - mover.Position).LengthSquared() >= mover.CalculateGravityRadiusSquared(1, Globals.MinimumGravityForCalculation))
                            continue;
                        mover.Attract(other);
                    }
                }

                if(timeRatio == 0)
                    _movers.RemoveAll(m => m.Radius is 0);
                
                for (var i = 0; i < _movers.Count; i++)
                    _movers[i].Update(time);
                
                for (var i = 0; i < _movers.Count; i++)
                    if (timeRatio > 0)
                        _movers[i].ResetAccelerations();
                
                _ship.Update(time);

                if(timeRatio > 0)
                    _ship.ResetAccelerations();
            }
            
            base.OnUpdating(time);
        }

        private void TogglePause(bool toggle = true)
        {
            _paused ^= toggle;
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            var oldFps = _fps;
            _fps = 1 / time.ElapsedTime.TotalSeconds;
            Debug.WriteLine($"FPS: {_fps}");
            // var fpsRatio = TargetElapsedTime / time.ElapsedTime;
            var window = Ultraviolet.GetPlatform().Windows.GetCurrent();
            window.Caption = $"{(_paused ? "||" : "")} {(int)_ups}ups - {(int)_fps}fps - {Globals.TimeRatio}s/s - {_movers.Count}planets";
            var selectedMover = SelectedMover;

            using (_draw.Start())
            {
                var windowSize = new Vector2(window.ClientSize.Width, window.ClientSize.Height);
                var centerWindow = windowSize / 2;
                var offset = TotalOffset - centerWindow;

                if (_inShip)
                {
                    window.Caption += $" -- {_ship.Mass}kg - {_ship.Velocity.Length():0.00000}m/s - {_ship.Acceleration.Length():0.00000}m/s/s - {_ship.Rotation:0.00000}rad - {_ship.AngleVelocity:0.00000}rad/s - {_ship.AngleAcceleration:0.00000}rad/s/s";
                    if(selectedMover is CelestialBody body)
                    {
                        window.Caption += $" -- {(_ship.Position - body.Position).Length():0.00}m - {(_ship.Velocity - body.Velocity).Length():0.00}m/s";
                    }
                }
                else
                {
                    window.Caption += selectedMover is CelestialBody m1 ? $" -- {m1.Mass}kg - {m1.Density:0.00000}kg/m3 - {m1.Volume}m3 - {m1.Diametre}m - {m1.Velocity.Length():0.00000}m/s - {m1.Acceleration.Length():0.00000}m/s/s" : "";
                }
                
                if (Ultraviolet.GetInput().GetGlobalActions().ShowGravity)
                {
                    if (_gravityField is (Rectangle rect, Color color)[,] field)
                    {
                        var width = _gravityField.GetLength(0);
                        var height = _gravityField.GetLength(1);
                        for (var x = 0; x < width; x++)
                            for (var y = 0; y < height; y++)
                            {
                                var info = field[x, y];
                                spriteBatch.Draw(Globals.Pixel, info.rect, info.color);
                            }
                    }
                    if (!_gravityThread.IsAlive)
                        _gravityThread.Start();
                }
                
                if (oldFps < 50 && _fps < 50)
                {
                    switch (Globals.TimeRatio)
                    {
                        case 1:
                            Globals.MaxPlanets1 = Globals.MaxPlanets2 = Globals.MaxPlanets3 = Globals.MaxPlanets4 = _movers.Count;
                            break;
                        case 2:
                            Globals.MaxPlanets2 = Globals.MaxPlanets3 = Globals.MaxPlanets4 = _movers.Count;
                            break;
                        case 4:
                            Globals.MaxPlanets3 = Globals.MaxPlanets4 = _movers.Count;
                            break;
                        case 8:
                            Globals.MaxPlanets4 = _movers.Count;
                            break;
                    }
                }
                
                var drawingArea = new RectangleF(Point2F.Zero, window.ClientSize);
                for (var i = 0; i < _movers.Count; i++)
                    if (drawingArea.Contains(_movers[i].Position - offset))
                        _movers[i].Draw(spriteBatch, offset, _inShip ? (Movable)_ship : selectedMover);
                _ship.Draw(spriteBatch, _inShip ? _ship.Position - centerWindow : offset, selectedMover);
                _ship.Draw(spriteBatch, new Vector2(100, 50), selectedMover, zoom: 4);
                selectedMover?.Draw(spriteBatch, new Vector2(100, 250), _inShip ? _ship : null, zoom: 2);
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
            var window = Ultraviolet.GetPlatform().Windows.GetPrimary().ClientSize;
            var velocity = 800;
            var velRatio = 100f;
            var worldSize = 5;
#if DEBUG
            var rng = new Random(0);
            var movers = new List<CelestialBody>(50);
#else
            var rng = new Random();
            var movers = new List<CelestialBody>(Globals.TimeRatio switch
            {
                1 => Globals.MaxPlanets1,
                2 => Globals.MaxPlanets2,
                4 => Globals.MaxPlanets3,
                8 => Globals.MaxPlanets4,
            });
#endif
            for (int i = 1; i < movers.Capacity; i++)
            {
                var mover = new CelestialBody(rng.Next(5, 35), (float)rng.NextDouble() * 35)
                {
                    Position = new Vector2(rng.Next(window.Width * -worldSize, window.Width * (worldSize+1)), rng.Next(window.Height * -worldSize, window.Height * (worldSize+1))),
                    Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / velRatio,
                    Texture = texture
                };
                movers.Add(mover);
            }
            var hole = new BlackHole(rng.Next(15, 50), (float)rng.NextDouble() * 35)
            {
                Position = new Vector2(rng.Next(window.Width * -worldSize, window.Width * (worldSize+1)), rng.Next(window.Height * -worldSize, window.Height * (worldSize+1))),
                // Velocity = new Vector2(rng.Next(-velocity, velocity), rng.Next(-velocity, velocity)) / (velRatio / 10),
                Texture = texture
            };
            movers.Add(hole);
            movers.Reverse();
            _movers = movers;
            _offset = Vector2.Zero;

            var selected = _movers[1];
            _ship.Position = selected.Position + -Vector2.UnitY * selected.Radius;
            _ship.Velocity = selected.Velocity;
            _ship.ResetAccelerations();
            _ship.Rotation = 0;
            _ship.AngleVelocity = 0;
            SelectedMover = selected;
        }

        private Vector2 TotalOffset
        {
            get => _inShip ? _ship.Position : ((SelectedMover is CelestialBody body ? body.Position : Vector2.Zero) + _offset);
        }

        private ContentManager contentManager = null!;
        private SpriteBatch spriteBatch = null!;
        private Texture2D texture = null!;
        private Ship _ship = null!;
        private List<CelestialBody> _movers = null!;
        private CelestialBody? SelectedMover
        {
            get => System.Linq.Enumerable.FirstOrDefault(_movers, m => m.Selected);
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
        private double _ups, _fps;
        private (Rectangle rect, Color color)[,] _gravityField = null!;
        private readonly Thread _gravityThread;
    }
}