using System;
using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Levels;

// We do NOT want to use LINQ in the collision engine. This class is a bottleneck and we need it to run efficiently.
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace AM2E.Collision;

// The Technical Debt That I Deservioli

public static class LOIC
{
    // TODO: These check all *active* colliders and don't filter per-room. That's likely bad... provide a way to do that instead.

    public static bool CheckPoint<T>(int x, int y) where T : ICollider
    {
        return ColliderAtPoint<T>(x, y) is not null;
    }

    public static T ColliderAtPoint<T>(int x, int y) where T : ICollider
    {
        foreach (ICollider collider in ActorManager.PersistentActors.Values)
        {
            // Return whether any collider is found that matches interface and contains the input point.
            if (InternalCheckPoint<T>(collider, x, y))
                return (T)collider;
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (ICollider collider in layer.Colliders)
                {
                    // Return whether any collider is found that matches interface and contains the input point.
                    if (InternalCheckPoint<T>(collider, x, y))
                        return (T)collider;
                }
            }
        }

        return default;
    }

    private static bool InternalCheckPoint<T>(ICollider collider, int x, int y) where T : ICollider
    {
        return (collider is T && collider.Collider.ContainsPoint<T>(x, y));
    }
    
    public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        return ColliderAtRectangle<T>(x1, y1, x2, y2) is not null;
    }

    public static T ColliderAtRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        // Return whether any collider is found that matches interface and is intersected by the input hitbox.
        foreach (ICollider collider in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                return (T)collider;
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (ICollider collider in layer.Colliders)
                {
                    if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                        return (T)collider;
                }
            }
        }
        
        return default;
    }

    public static IEnumerable<T> AllCollidersAtRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        var output = new List<T>();
    
        foreach (ICollider collider in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                output.Add((T)collider);
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (ICollider collider in layer.Colliders)
                {
                    if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                    {
                        var col = (T)collider;
                        if (!output.Contains(col))
                            output.Add(col);
                    }
                }
            }
        }

        return output;
    }

    public static bool CheckLine<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        return ColliderAtLine<T>(x1, y1, x2, y2) is not null;
    }

    public static T ColliderAtLine<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        foreach (ICollider collider in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckLine<T>(collider, x1, y1, x2, y2))
                return (T)collider;
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (ICollider collider in layer.Colliders)
                {
                    if (InternalCheckLine<T>(collider, x1, y1, x2, y2))
                        return (T)collider;
                }
            }
        }
        
        return default;
    }

    private static bool InternalCheckLine<T>(ICollider collider, int x1, int y1, int x2, int y2) where T : ICollider
    {
        if (collider is not T)
            return false;

        return collider.Collider.IsIntersectedByLine<T>(x1, y1, x2, y2);
    }

    // Static hitbox to save on instantiation/garbage collector spam.
    private static readonly RectangleHitbox RectCheckHitbox = new RectangleHitbox(0, 0, 1, 1);
    private static bool InternalCheckRectangle<T>(ICollider collider, int x1, int y1, int x2, int y2) where T : ICollider
    {
        if (collider is not T)
            return false;
        
        // Update hitbox.
        RectCheckHitbox.X = x1;
        RectCheckHitbox.Y = y1;
        RectCheckHitbox.Resize((x2 - x1) + 1, (y2 - y1) + 1);

        return collider.Collider.IsIntersectedBy<T>(RectCheckHitbox);
    }

    public static ICollider CheckCollider<T>(Collider self) where T : ICollider
    {
        // Return first (or null) Collider that matches interface and is intersected by input Collider.
        foreach (var actor in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckCollider<T>(actor, self))
                return actor;
        }

        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (var collider in layer.Colliders)
                {
                    if (InternalCheckCollider<T>(collider, self))
                        return collider;
                }
            }
        }

        return null;
    }

    public static IEnumerable<T> CheckAllColliders<T>(Collider self) where T : ICollider
    {
        var output = new List<T>();

        // TODO: Put this into an r-tree too somehow?
        foreach (ICollider collider in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckCollider<T>(collider, self))
                output.Add((T)collider);
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            var check = level.RTree.Intersects(self.Bounds);
            foreach (var collider in check)
            {
                if (InternalCheckCollider<T>(collider, self))
                {
                    var col = (T)collider;
                    if (!output.Contains(col))
                        output.Add(col);
                }
            }
        }

        /*
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (ICollider collider in layer.Colliders)
                {
                    if (InternalCheckCollider<T>(collider, self))
                    {
                        var col = (T)collider;
                        if (!output.Contains(col))
                            output.Add(col);
                    }
                }
            }
        }
        */
        
        return output;
    }

    private static bool InternalCheckCollider<T>(ICollider collider, Collider self) where T : ICollider
    {
        if (collider is not T || collider.Collider == self)
            return false;

        return self.Intersects<T>(collider.Collider);
    }
}