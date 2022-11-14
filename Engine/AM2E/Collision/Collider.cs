using GameContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

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
        private enum Axis
        {
            X,
            Y
        }

        public int X 
        { 
            get
            {
                return Hitbox.X;
            }
            set
            {
                Hitbox.X = value;
            } 
        }
        public int Y 
        { 
            get
            {
                return Hitbox.Y;
            }
            set
            {
                Hitbox.Y = value;
            }
        }

        // TODO: Make these two whine when accessed outside of a collision?
        public int VelX
        {
            get
            {
                return vel[0];
            }
        }

        public int VelY
        {
            get
            {
                return vel[1];
            }
        }

        private readonly ArrayList events = new();
        private readonly ArrayList types = new();

        private int checkX = 0;
        private int checkY = 0;

        private static CollisionDirection[] dirsH = new CollisionDirection[] { CollisionDirection.Left, CollisionDirection.None, CollisionDirection.Right };
        private static CollisionDirection[] dirsV = new CollisionDirection[] { CollisionDirection.Up, CollisionDirection.None, CollisionDirection.Down };

        private bool[] continueMovement = new bool[] { false, false };
        public double SubVelX
        {
            get { return subVel[0]; }
            set { subVel[0] = value; }
        }
        public double SubVelY
        {
            get { return subVel[1]; }
            set { subVel[1] = value; }
        }

        private double[] subVel = new double[] { 0d, 0d };
        private int[] vel = new int[] { 0, 0 };

        private bool inMovement = false;

        public Action OnSubstep { get; set; }
        public Action AfterSubstep { get; set; }

        public CollisionDirection Direction { get; private set; } = CollisionDirection.None;
        public Hitbox Hitbox { get; private set; }

        public Collider(Hitbox hitbox)
        {
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

            int x = (int)Axis.X;
            int y = (int)Axis.Y;
            
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

            bool subIncrement = false;
            int domCurrent = 0;
            int subCurrent = 0;
            int subCurrentLast;

            int domAbs = Math.Abs(vel[domAxis]);
            int subAbs = Math.Abs(vel[subAxis]);

            int domMult, subMult;

            int[] sign = new int[] { Math.Sign(vel[x]), Math.Sign(vel[y]) };

            for (var i = 0; i < domAbs; ++i)
            {
                if (continueMovement[domAxis] && (domCurrent != vel[domAxis]))
                {
                    Direction = (domAxis == x) ? dirsH[sign[x] + 1] : dirsV[sign[y] + 1];

                    CheckAndRunAll((domAxis == x) ? sign[x] : 0, (domAxis == y) ? sign[y] : 0);

                    // Process position
                    domMult = continueMovement[domAxis] ? sign[domAxis] : 0;
                    domCurrent += domMult;

                    if (domAxis == x)
                        X += domMult;
                    else
                        Y += domMult;

                    subCurrentLast = subCurrent;

                    // TODO: This might need to be floats and then floored, but it also might work fine like this. Maybe.
                    subCurrent = vel[subAxis] * domCurrent / vel[domAxis];
                    subIncrement = (subCurrent != subCurrentLast);
                    subCurrent = subCurrentLast; // Prevents stupid slidey shenanigans. At least that's what I said in the LHC...
                }

                if ((subIncrement || !continueMovement[domAxis]) && continueMovement[subAxis] && (subCurrent != vel[subAxis]))
                {
                    Direction = (subAxis == x) ? dirsH[sign[x] + 1] : dirsV[sign[y] + 1];

                    CheckAndRunAll((subAxis == x) ? sign[x] : 0, (subAxis == y) ? sign[y] : 0);

                    // Process position
                    subMult = continueMovement[subAxis] ? sign[subAxis] : 0;
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
            return Direction == CollisionDirection.Left || Direction == CollisionDirection.Right;
        }

        public bool DirectionVertical()
        {
            return Direction == CollisionDirection.Up || Direction == CollisionDirection.Down;
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

            if (col != null)
            {
                foreach (object ob in events)
                {
                    if (ob is Action<T>)
                    {
                        var ev = ob as Action<T>;
                        ev((T)col);
                        return;
                    }
                }
            }
        }
        
        public ICollider Check<T>(int x, int y) where T : ICollider
        {
            int _x = X;
            int _y = Y;
            X = x;
            Y = y;

            ICollider output = LOIC.CheckCollider<T>(this);

            X = _x;
            Y = _y;

            return output;
        }
    }
}
