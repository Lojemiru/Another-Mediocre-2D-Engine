﻿using System.Collections.Generic;
using AM2E.Actors;
using AM2E.Levels;

// We do NOT want to use LINQ in the collision engine. This class is a bottleneck and we need it to run efficiently.
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace AM2E.Collision;

// The Technical Debt That I Deservioli

public static class LOIC
{
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
        foreach (var collider in ActorManager.PersistentActors.Values)
        {
            // Return whether any collider is found that matches interface and contains the input point.
            if (InternalCheckPoint<T>(collider, x, y))
                return true;
        }
        
        foreach (var level in World.ActiveLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (var collider in layer.Colliders)
                {
                    // Return whether any collider is found that matches interface and contains the input point.
                    if (InternalCheckPoint<T>(collider, x, y))
                        return true;
                }
            }
        }

        return false;
    }

    private static bool InternalCheckPoint<T>(ICollider collider, int x, int y) where T : ICollider
    {
        return (collider is T && collider.Collider.ContainsPoint<T>(x, y));
    }
    
    public static bool CheckRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        // Return whether any collider is found that matches interface and is intersected by the input hitbox.
        foreach (var collider in ActorManager.PersistentActors.Values)
        {
            if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                return true;
        }
        
        foreach (var level in World.LoadedLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (var collider in layer.Colliders)
                {
                    if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                        return true;
                }
            }
        }

        return false;
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

    private static bool InternalCheckCollider<T>(ICollider collider, Collider self) where T : ICollider
    {
        if (collider is not T)
            return false;

        return self.Intersects<T>(collider.Collider);
    }
}