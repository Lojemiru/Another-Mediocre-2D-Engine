using GameContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AM2E.Collision
{
    // The Technical Debt That I Deservioli
    
    // TODO: Consider whether the LINQ queries here should be replaced with simple iterators for performance.
    //       This is likely going to be the biggest performance bottleneck after rendering and level loading.
    public static class LOIC
    {
        private static List<ICollider> colliders = new();

        public static void Register(ICollider collider)
        {
            colliders.Add(collider);
        }
        
        // TODO: De-registration
        // TODO: "active" pools (so we can load but not check against inactive rooms etc.)

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
            // Return whether any collider is found that matches interface and contains the input point.
            foreach (var col in colliders)
            {
                if (col is T && col.Collider.ContainsPoint<T>(x, y))
                    return true;
            }

            return false;
        }
        
        // Static hitbox to save on instantiation/garbage collector spam.
        private static RectangleHitbox rectCheckHitbox = new RectangleHitbox(0, 0, 1, 1);
        public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
        {
            // Update hitbox.
            rectCheckHitbox.X = x1;
            rectCheckHitbox.Y = y1;
            rectCheckHitbox.Resize((x2 - x1) + 1, (y2 - y1) + 1);

            // Return whether any collider is found that matches interface and is intersected by the input hitbox.
            foreach (var col in colliders)
            {
                if (col is T && col.Collider.IsIntersectedBy<T>(rectCheckHitbox))
                    return true;
            }

            return false;
        }

        public static ICollider CheckCollider<T>(Collider self) where T : ICollider
        {
            // Return first (or null) Collider that matches interface and is intersected by input Collider.
            foreach (var col in colliders)
            {
                if (col is T && self.Intersects<T>(col.Collider))
                    return col;
            }

            return null;
        }
    }

}
