using System;
using System.Collections.Generic;

namespace AM2E.Actors;

public static class PlayerPool
{
    private static readonly List<IPlayer> Players = new();
    
    public enum Side
    {
        Right,
        Down,
        Left,
        Up
    }
    
    /// <summary>
    /// Adds the given <see cref="IPlayer"/> to the pool.
    /// </summary>
    /// <param name="player"></param>
    public static void Add(IPlayer player)
    {
        if (!Players.Contains(player))
            Players.Add(player);
    }

    /// <summary>
    /// Removes the given <see cref="IPlayer"/> from the pool.
    /// </summary>
    /// <param name="player"></param>
    public static void Remove(IPlayer player)
    {
        if (Players.Contains(player))
            Players.Remove(player);
    }

    /// <summary>
    /// Removes all <see cref="IPlayer"/>s from the pool.
    /// </summary>
    public static void Empty()
    {
        Players.Clear();
    }
    
    /// <summary>
    /// Returns the first <see cref="IPlayer"/> in the pool.
    /// </summary>
    public static IPlayer GetFirst()
    {
        return (Players.Count > 0) ? Players[0] : null;
    }
    
    /// <summary>
    /// Returns the closest <see cref="IPlayer"/> in the pool to the given position.
    /// </summary>
    /// <param name="x">X position to base the check on.</param>
    /// <param name="y">Y position to base the check on.</param>
    public static IPlayer GetClosest(int x, int y)
    {
        if (Players.Count <= 1)
            return GetFirst();

        var closest = Players[0];
        var closestX = Math.Abs(closest.X - x);
        var closestY = Math.Abs(closest.Y - y);

        for (var i = 1; i < Players.Length(); i++)
        {
            if (Math.Abs(Players[i].X - x) >= closestX || Math.Abs(Players[i].Y - y) >= closestY)
                continue;
            
            closest = Players[i];
            closestX = Math.Abs(closest.X - x);
            closestY = Math.Abs(closest.Y - y);
        }

        return closest;
    }

    /// <summary>
    /// Finds the closest <see cref="IPlayer"/> to the given position on the given <see cref="Side"/>.
    /// </summary>
    /// <param name="x">X position to base the check on.</param>
    /// <param name="y">Y position to base the check on.</param>
    /// <param name="side">The side to check.</param>
    /// <returns>The closest <see cref="IPlayer"/> on the given <see cref="Side"/>, if it exists; otherwise null.</returns>
    public static IPlayer GetClosestOnSide(int x, int y, Side side)
    {
        if (Players.Count < 1)
            return null;
        
        IPlayer closest = null;
        
        foreach (var player in Players)
        {
            switch (side)
            {
                case Side.Right:
                    if (player.X >= x && (closest == null || closest.X > player.X))
                        closest = player;
                    break;
                case Side.Down:
                    if (player.Y >= y && (closest == null || closest.Y > player.Y))
                        closest = player;
                    break;
                case Side.Left:
                    if (player.X <= x && (closest == null || closest.X < player.X))
                        closest = player;
                    break;
                case Side.Up:
                    if (player.Y <= y && (closest == null || closest.Y < player.Y))
                        closest = player;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
        
        return closest;
    }
}