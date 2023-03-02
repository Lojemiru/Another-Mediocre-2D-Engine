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
            return colliders.Any(col => col is T && col.Collider.ContainsPoint<T>(x, y));
        }

        public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
        {
            // TODO: Convert hitbox to const for performance?
            var hitbox = new RectangleHitbox(x1, y1, (x2 - x1) + 1, (y2 - y1) + 1);
            // Return whether any collider is found that matches interface and is intersected by the input hitbox.
            return colliders.Any(col => col is T && col.Collider.IsIntersectedBy<T>(hitbox));
        }

        public static ICollider CheckCollider<T>(Collider self) where T : ICollider
        {
            // Return first (or null) Collider that matches interface and is intersected by input Collider.
            return colliders.FirstOrDefault(col => col is T && self.Intersects<T>(col.Collider));
        }
    }

}
