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
        public readonly string ID;
        public bool FlippedX { get; private set; } = false;
        public bool FlippedY { get; private set; } = false;
        
        public static readonly Hitbox DefaultHitbox = new RectangleHitbox(0, 0, 16, 16);
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
        // TODO: This could be desynced from actual layer bindings, but has to be publicly settable for Layers to handle it... figure something out.
        public Layer Layer;
        public Collider Collider { get; }
    }
}
