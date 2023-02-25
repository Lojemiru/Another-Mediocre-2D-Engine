using GameContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Collision
{
    // The Technical Debt That I Deservioli
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
        
        // TODO: This feels redundant with Collider.Check<T>()... Replace? Would be nice for it not to touch the LOIC on the user end.
        public static bool Check<T>(Collider self, int x, int y) where T : ICollider
        {
            var prevX = self.X;
            var prevY = self.Y;
            self.X = x;
            self.Y = y;
            var output = Check<T>(self);
            self.X = prevX;
            self.Y = prevY;
            return output;
        }

        public static bool CheckPoint<T>(int x, int y) where T : ICollider
        {
            foreach (var col in colliders)
            {
                if (col is T && col.Collider.ContainsPoint<T>(x, y))
                    return true;
            }
            return false;
        }

        public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
        {
            var hitbox = new RectangleHitbox(x1, y1, (x2 - x1) + 1, (y2 - y1) + 1);
            foreach (var col in colliders)
            {
                if (col is T && col.Collider.IsIntersectedBy<T>(hitbox))
                    return true;
            }
            return false;
        }

        public static ICollider CheckCollider<T>(Collider self) where T : ICollider
        {
            foreach (var col in colliders)
            {
                if (col is T && self.Intersects<T>(col.Collider))
                    return col;
            }
            return null;
        }
    }

}
