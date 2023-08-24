using System.Collections.Generic;
using RTree;

// We do NOT want to use LINQ in the collision engine. This class is a bottleneck and we need it to run efficiently.
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace AM2E.Collision;

// The Technical Debt That I Deservioli

public static class LOIC
{
    internal static RTree<ICollider> RTree = new();

    public static bool CheckPoint<T>(int x, int y) where T : ICollider
    {
        return ColliderAtPoint<T>(x, y) is not null;
    }

    public static T ColliderAtPoint<T>(int x, int y) where T : ICollider
    {
        // TODO: Modify RTree to allow for checking points directly
        var check = RTree.Intersects(new Rectangle(x - 1, y - 1, x + 1, y + 1));
        foreach (var collider in check)
        {
            if (InternalCheckPoint<T>(collider, x, y))
                return (T)collider;
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
        var check = RTree.Intersects(new Rectangle(x1, y1, x2, y2));
        foreach (var collider in check)
        {
            if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                return (T)collider;
        }

        return default;
    }

    public static IEnumerable<T> AllCollidersAtRectangle<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        var output = new List<T>();
        
        var check = RTree.Intersects(new Rectangle(x1, y1, x2, y2));
        foreach (var collider in check)
        {
            if (InternalCheckRectangle<T>(collider, x1, y1, x2, y2))
                output.Add((T)collider);
        }

        return output;
    }

    public static bool CheckLine<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        return ColliderAtLine<T>(x1, y1, x2, y2) is not null;
    }
    
    public static T ColliderAtLine<T>(int x1, int y1, int x2, int y2) where T : ICollider
    {
        // TODO: Write custom method for checking a line within the tree??? This is going to hit a LOT of things we don't need to consider...
        var check = RTree.Intersects(new Rectangle(x1, y1, x2, y2));
        foreach (var collider in check)
        {
            if (InternalCheckLine<T>(collider, x1, y1, x2, y2))
                return (T)collider;
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
        var check = RTree.Intersects(self.Bounds);
        foreach (var collider in check)
        {
            if (InternalCheckCollider<T>(collider, self))
                return collider;
        }

        return null;
    }

    public static IEnumerable<T> CheckAllColliders<T>(Collider self) where T : ICollider
    {
        var output = new List<T>();

        var check = RTree.Intersects(self.Bounds);
        foreach (var collider in check)
        {
            if (!InternalCheckCollider<T>(collider, self)) 
                continue;
            
            output.Add((T)collider);
        }

        return output;
    }

    private static bool InternalCheckCollider<T>(ICollider collider, Collider self) where T : ICollider
    {
        if (collider is not T || collider.Collider == self)
            return false;

        return self.Intersects<T>(collider.Collider);
    }
}