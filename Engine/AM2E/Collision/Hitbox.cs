using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AM2E.Graphics;

namespace AM2E.Collision
{
    public abstract class Hitbox
    {
        public int OffsetX { get; protected set; } = 0;
        public int OffsetY { get; protected set; } = 0;
        protected int InitialOffsetX = 0;
        protected int InitialOffsetY = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public abstract int BoundLeft { get; }
        public abstract int BoundRight { get; }
        public abstract int BoundTop { get; }
        public abstract int BoundBottom { get; }

        public bool FlippedX { get; private set; } = false;
        public bool FlippedY { get; private set; } = false;

        public void ApplyFlipsFromBits(int bits)
        {
            ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
        }
        public virtual void ApplyFlips(bool xFlip, bool yFlip)
        {
            FlippedX = xFlip;
            FlippedY = yFlip;
        }
        
        private List<Type> interfaces;

        public bool MatchesInterface(Type type)
        {
            if (interfaces == null)
                return true;
            
            // Return: interfaces contains type, or interfaces contains any of type's types
            var typeInterfaces = type.GetInterfaces();
            return interfaces.Any(x => x == type || typeInterfaces.Any(y => x == y));
        }

        public void BindInterface(Type type)
        {
            interfaces ??= new List<Type>();
            interfaces.Add(type);
        }

        protected abstract bool Intersects(RectangleHitbox hitbox);
        protected abstract bool Intersects(CircleHitbox hitbox);
        protected abstract bool Intersects(PreciseHitbox hitbox);
        public abstract bool ContainsPoint(int x, int y);
        public bool Intersects(Hitbox hitbox)
        {
            return hitbox switch
            {
                // Check Precise first so that we don't accidentally grab them as Rectangles :D
                PreciseHitbox preciseHitbox => Intersects(preciseHitbox),
                RectangleHitbox rectangleHitbox => Intersects(rectangleHitbox),
                CircleHitbox circleHitbox => Intersects(circleHitbox),
                _ => throw new NotImplementedException("Hitbox type " + hitbox.GetType() + " is not yet implemented!!!")
            };
        }
    }

    // TODO: Move these to their own files :frog:
    public class RectangleHitbox : Hitbox
    {
        public int Width { get; }
        public int Height { get; }

        public override int BoundLeft => X - OffsetX;

        public override int BoundRight => BoundLeft + Width - 1;

        public override int BoundTop => Y - OffsetY;

        public override int BoundBottom => BoundTop + Height - 1;

        public RectangleHitbox(int x, int y, int w, int h, int offsetX = 0, int offsetY = 0)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            OffsetX = InitialOffsetX = offsetX;
            OffsetY = InitialOffsetY = offsetY;
        }

        public override void ApplyFlips(bool xFlip, bool yFlip)
        {
            base.ApplyFlips(xFlip, yFlip);
            OffsetX = FlippedX ? (Width - 1) - InitialOffsetX : InitialOffsetX;
            OffsetY = FlippedY ? (Height - 1) - InitialOffsetY : InitialOffsetY;
        }

        protected override bool Intersects(RectangleHitbox hitbox)
        {
            // TODO: There's probably a better way to structure this boolean check for speed. If we care.
            return !(BoundRight < hitbox.BoundLeft || hitbox.BoundRight < BoundLeft || BoundBottom < hitbox.BoundTop || hitbox.BoundBottom < BoundTop);
        }

        protected override bool Intersects(CircleHitbox hitbox)
        {
            // Only two conditions: circle center is in rectangle, or endpoint of radius is in rectangle
            if (ContainsPoint(hitbox.X, hitbox.Y)) return true;

            var centerX = (float)(BoundLeft + BoundRight) / 2;
            var centerY = (float)(BoundTop + BoundBottom) / 2;

            var pos = new Vector2(centerX - hitbox.X, centerY - hitbox.Y);
            if ((int)pos.Length() > hitbox.Radius) 
            {
                pos.Normalize();
                pos *= hitbox.Radius;
            }

            return ContainsPoint((int)(hitbox.X - pos.X), (int)(hitbox.Y - pos.Y));
        }

        protected override bool Intersects(PreciseHitbox hitbox)
        {
            return hitbox.Intersects(this);
        }

        public override bool ContainsPoint(int x, int y)
        {
            return !(x < BoundLeft || x > BoundRight || y < BoundTop || y > BoundBottom);
        }
    }

    // TODO: Implement rotating rectangles :)

    public class CircleHitbox : Hitbox
    {
        public int Radius { get; }

        public override int BoundLeft => X - Radius;

        public override int BoundRight => X + Radius;

        public override int BoundTop => Y - Radius;

        public override int BoundBottom => Y + Radius;

        protected override bool Intersects(RectangleHitbox hitbox)
        {
            return hitbox.Intersects(this);
        }

        protected override bool Intersects(CircleHitbox hitbox)
        {
            // Add radii, compare to distance between both centers
            return (Radius + hitbox.Radius) >= PointDistance(X, Y, hitbox.X, hitbox.Y);
        }

        protected override bool Intersects(PreciseHitbox hitbox)
        {
            return hitbox.Intersects(this);
        }

        public override bool ContainsPoint(int x, int y)
        {
            return PointDistance(X, Y, x, y) <= Radius;
        }

        public static int PointDistance(int x1, int y1, int x2, int y2)
        {
            return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }

    // Inherits from RectangleHitbox for the bounds getters
    public class PreciseHitbox : RectangleHitbox
    {
        public bool[, ] Mask { get; }

        // TODO: Alt constructor that turns an image into the mask, or do we process that somewhere above?
        public PreciseHitbox(int x, int y, bool[, ] mask, int offsetX = 0, int offsetY = 0) : base(x, y, 
            mask.GetLength(0), mask.GetLength(1), offsetX, offsetY)
        {
            Mask = mask;
        }

        protected override bool Intersects(RectangleHitbox hitbox)
        {
            if (!base.Intersects(hitbox))
                return false;
            
            var startX = Math.Clamp(hitbox.BoundLeft - BoundLeft, 0, Width - 1);
            var startY = Math.Clamp(hitbox.BoundTop - BoundTop, 0, Height - 1);
            var endX = Math.Clamp(hitbox.BoundRight - BoundLeft + 1, 0, Width);
            var endY = Math.Clamp(hitbox.BoundBottom - BoundTop + 1, 0, Height);

            // TODO: Should this check be interlaced? Might speed up some use cases, but wouldn't affect our worst-case scenario :/
            for (var i = startX; i < endX; ++i)
            {
                for (var j = startY; j < endY; ++j)
                {
                    if (CheckPointInMask(i, j))
                        return true;
                }
            }
            
            return false;
        }

        protected override bool Intersects(CircleHitbox hitbox)
        {
            // Early exit - return false if bounds don't even overlap
            // TODO: Turn this into a shared/more generic check kthx :)
            if (BoundRight < hitbox.BoundLeft || hitbox.BoundRight < BoundLeft || BoundBottom < hitbox.BoundTop || hitbox.BoundBottom < BoundTop)
                return false;

            var startX = Math.Clamp(hitbox.BoundLeft, 0, Width - 1);
            var startY = Math.Clamp(hitbox.BoundTop, 0, Height - 1);
            var endX = Math.Clamp(hitbox.BoundRight, 0, Width);
            var endY = Math.Clamp(hitbox.BoundBottom, 0, Height);

            for (var i = startX; i < endX; ++i)
            {
                for (var j = startY; j < endY; ++j)
                {
                    // TODO: Does not currently account for flips
                    if (Mask[i, j] && hitbox.ContainsPoint(i, j)) 
                       return true;
                }
            }

            return false;
        }

        protected override bool Intersects(PreciseHitbox hitbox)
        {
            // Early exit - return false if bounds don't even overlap
            return !base.Intersects((RectangleHitbox)hitbox) &&
                   // TODO: Based on rectangle -> precise check testing, this might be slightly scuffed. Give it a proper test.
                   MaskIntersects(hitbox, hitbox.BoundLeft - BoundLeft, hitbox.BoundTop - BoundTop);
        }

        public override bool ContainsPoint(int x, int y)
        {
            // Check base, then return value of array cell
            return base.ContainsPoint(x, y) && CheckPointInMask(x - (X - OffsetX), y - (Y - OffsetY));
        }
        
        private bool CheckPointInMask(int x, int y)
        {
            return Mask[FlippedX ? (Width - 1) - x : x, FlippedY ? (Height - 1) - y : y];
        }

        public bool MaskIntersects(PreciseHitbox hitbox, int offsetX, int offsetY)
        {
            var startX = Math.Clamp(offsetX, 0, Width - 1);
            var startY = Math.Clamp(offsetY, 0, Height - 1);
            var endX = Math.Clamp(offsetX + hitbox.Mask.GetLength(0) - 1, 0, Width);
            var endY = Math.Clamp(offsetY + hitbox.Mask.GetLength(1) - 1, 0, Height);

            for (var i = startX; i < endX; ++i)
            {
                for (var j = startY; j < endY; ++j)
                {
                    if (CheckPointInMask(i, j) && hitbox.CheckPointInMask(i, j))
                        return true;
                }
            }

            return false;
        }
    }
}
