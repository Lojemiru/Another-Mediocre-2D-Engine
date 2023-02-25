using GameContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using LanguageExt;

namespace AM2E.Collision
{
    public enum CollisionDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }
    public class Collider
    {
        private enum Axis : int
        {
            X,
            Y
        }

        private int _x;
        private int _y;
        public int X
        {
            get => _x;
            set
            {
                _x = value;
                if (Hitbox != null) Hitbox.X = value;
            }
        }
        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                if (Hitbox != null) Hitbox.Y = value;
            }
        }
        
        // TODO: Make these two whine when accessed outside of a collision?
        public int VelX => vel[0];

        public int VelY => vel[1];

        private readonly ArrayList events = new();
        private readonly ArrayList types = new();

        private int checkX = 0;
        private int checkY = 0;

        private static readonly CollisionDirection[] DirsH = { CollisionDirection.Left, CollisionDirection.None, CollisionDirection.Right };
        private static readonly CollisionDirection[] DirsV = { CollisionDirection.Up, CollisionDirection.None, CollisionDirection.Down };

        private bool[] continueMovement = { false, false };
        public double SubVelX
        {
            get => subVel[0];
            set => subVel[0] = value;
        }
        public double SubVelY
        {
            get => subVel[1];
            set => subVel[1] = value;
        }

        private readonly double[] subVel = { 0d, 0d };
        private readonly int[] vel = { 0, 0 };

        private bool inMovement = false;

        public Action OnSubstep { get; set; }
        public Action AfterSubstep { get; set; }

        public CollisionDirection Direction { get; private set; } = CollisionDirection.None;
        public Hitbox Hitbox { get; private set; }
        
        public bool FlippedX { get; protected set; } = false;
        public bool FlippedY { get; protected set; } = false;

        public void ApplyFlipsFromBits(int bits)
        {
            ApplyFlips((bits & 1) != 0, (bits & 2) != 0);
        }
        public virtual void ApplyFlips(bool xFlip, bool yFlip)
        {
            FlippedX = xFlip;
            FlippedY = yFlip;
            Hitbox.ApplyFlips(FlippedX, FlippedY);
        }

        public Collider(int x, int y, Hitbox hitbox)
        {
            X = x;
            Y = y;
            Hitbox = hitbox;
        }

        public void Add<T>(Action<T> callback) where T : ICollider
        {
            events.Add(callback);
            types.Add(typeof(T));
        }

        public void MoveAndCollide(double xVel, double yVel)
        {
            inMovement = true;

            const int x = (int)Axis.X;
            const int y = (int)Axis.Y;
            
            // Add velocity to subVel
            subVel[x] += xVel;
            subVel[y] += yVel;

            // Cast off decimals
            vel[x] = (int)subVel[x];
            vel[y] = (int)subVel[y];

            // Reduce subVel by the integer amount we removed
            subVel[x] -= vel[x];
            subVel[y] -= vel[y];

            // Determine dominant and subordinate axis
            int domAxis = x,
                subAxis = y;

            if (Math.Abs(vel[y]) > Math.Abs(vel[x]))
            {
                domAxis = y;
                subAxis = x;
            }

            var subIncrement = false;
            var domCurrent = 0;
            var subCurrent = 0;

            var domAbs = Math.Abs(vel[domAxis]);
            var subAbs = Math.Abs(vel[subAxis]);

            var sign = new int[] { Math.Sign(vel[x]), Math.Sign(vel[y]) };

            for (var i = 0; i < domAbs; ++i)
            {
                if (continueMovement[domAxis] && (domCurrent != vel[domAxis]))
                {
                    Direction = (domAxis == x) ? DirsH[sign[x] + 1] : DirsV[sign[y] + 1];

                    CheckAndRunAll((domAxis == x) ? sign[x] : 0, (domAxis == y) ? sign[y] : 0);

                    // Process position
                    var domMult = continueMovement[domAxis] ? sign[domAxis] : 0;
                    domCurrent += domMult;

                    if (domAxis == x)
                        X += domMult;
                    else
                        Y += domMult;

                    var subCurrentLast = subCurrent;

                    // TODO: This might need to be floats and then floored, but it also might work fine like this. Maybe.
                    subCurrent = vel[subAxis] * domCurrent / vel[domAxis];
                    subIncrement = (subCurrent != subCurrentLast);
                    subCurrent = subCurrentLast; // Prevents stupid slidey shenanigans. At least that's what I said in the LHC...
                }

                if ((subIncrement || !continueMovement[domAxis]) && continueMovement[subAxis] && (subCurrent != vel[subAxis]))
                {
                    Direction = (subAxis == x) ? DirsH[sign[x] + 1] : DirsV[sign[y] + 1];

                    CheckAndRunAll((subAxis == x) ? sign[x] : 0, (subAxis == y) ? sign[y] : 0);

                    // Process position
                    var subMult = continueMovement[subAxis] ? sign[subAxis] : 0;
                    subCurrent += subMult;

                    if (subAxis == x)
                        X += subMult;
                    else
                        Y += subMult;
                }

                AfterSubstep();

                // Early exit if stopped on both axes
                if (!continueMovement[x] && !continueMovement[y])
                    break;
            }

            // Reset movement variables
            Direction = CollisionDirection.None;
            continueMovement[x] = true;
            continueMovement[y] = true;

            inMovement = false;
        }

        public void StopX()
        {
            if (!inMovement)
                throw new InvalidOperationException("Stop calls may only be called from a collision callback!");

            continueMovement[0] = false;
        }

        public void StopY()
        {
            if (!inMovement)
                throw new InvalidOperationException("Stop calls may only be called from a collision callback!");

            continueMovement[1] = false;
        }

        public void Stop()
        {
            StopX();
            StopY();
        }

        public bool DirectionHorizontal()
        {
            return Direction is CollisionDirection.Left or CollisionDirection.Right;
        }

        public bool DirectionVertical()
        {
            return Direction is CollisionDirection.Up or CollisionDirection.Down;
        }

        private void CheckAndRunAll(int x, int y)
        {
            checkX = x + X;
            checkY = y + Y;

            OnSubstep();
        }

        public void CheckAndRun<T>() where T : ICollider
        {
            ICollider col = Check<T>(checkX, checkY);

            if (col == null) return;
            foreach (var ob in events)
            {
                if (ob is not Action<T> ev) continue;
                ev((T)col);
                return;
            }
        }
        
        public ICollider Check<T>(int x, int y) where T : ICollider
        {
            var _x = X;
            var _y = Y;
            X = x;
            Y = y;

            var output = LOIC.CheckCollider<T>(this);

            X = _x;
            Y = _y;

            return output;
        }

        public bool Intersects(Collider col)
        {
            return col.Hitbox.Intersects(Hitbox);
        }

        public bool ContainsPoint(int x, int y)
        {
            return Hitbox.ContainsPoint(x, y);
        }

        public bool Intersects(Hitbox hitbox)
        {
            return Hitbox.Intersects(hitbox);
        }
    }
}
