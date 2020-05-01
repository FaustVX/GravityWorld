using System;
using Ultraviolet;

namespace test1
{
    public abstract class Positionable
    {
        public Vector2 Position { get; set; }

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
    }
}