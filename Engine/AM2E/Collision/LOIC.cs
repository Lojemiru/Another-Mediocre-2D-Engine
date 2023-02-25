using GameContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Collision
{
    // AKA The Technical Debt That I Deservioli
    public static class LOIC
    {
        private static List<ICollider> colliders = new();

        public static void Register(ICollider collider)
        {
            colliders.Add(collider);
        }

        public static bool Check<T>(Collider self) where T : ICollider
        {
            return CheckCollider<T>(self) != null;
        }

        public static bool Check<T>(Collider self, int x, int y) where T : ICollider
        {
            var _x = self.X;
            var _y = self.Y;
            self.X = x;
            self.Y = y;
            bool output = Check<T>(self);
            self.X = _x;
            self.Y = _y;
            return output;
        }

        public static bool CheckPoint<T>(int x, int y) where T : ICollider
        {
            foreach (ICollider col in colliders)
            {
                if (col is T && col.Collider.ContainsPoint(x, y))
                    return true;
            }
            return false;
        }

        public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
        {
            var hitbox = new RectangleHitbox(x1, y1, (x2 - x1) + 1, (y2 - y1) + 1);
            foreach (ICollider col in colliders)
            {
                if (col is T && col.Collider.Intersects(hitbox))
                    return true;
            }
            return false;
        }

        public static ICollider CheckCollider<T>(Collider self) where T : ICollider
        {
            foreach (ICollider col in colliders)
            {
                if (col is T && col.Collider.Intersects(self))
                    return col;
            }
            return null;
        }
    }

}
