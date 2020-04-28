﻿using Ultraviolet;

namespace test1
{
    public abstract class Movable : Positionable
    {
        public Vector2 Velocity { get; set; }

        public virtual void Update()
        {
            Position += Velocity;
        }
    }
}
