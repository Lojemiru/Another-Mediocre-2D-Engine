using AM2E.Collision;
using AM2E.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using AM2E.Levels;

namespace AM2E.Actors
{
    // TODO: Need to implement IDisposable?
    public abstract partial class Actor
    {
        public int X { 
            get
            {
                return Collider.X;
            } 
            set
            {
                Collider.X = value;
            }
        }
        public int Y {
            get
            {
                return Collider.Y;
            }
            set
            {
                Collider.Y = value;
            }
        }
        public bool Persistent { get; set; } = false;
        public bool Exists { get; private set; } = true;
        public Layer Layer;
        public Collider Collider { get; }
    }
}
