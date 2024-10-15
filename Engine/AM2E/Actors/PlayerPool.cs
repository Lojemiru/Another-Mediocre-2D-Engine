using System;
using System.Collections.Generic;
using AM2E.Levels;

namespace AM2E.Actors;

public static class PlayerPool<T> where T : Actor
{
    private static readonly List<T> Players = new();

    public static T LocalPlayer;
    
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
    public static void Add(T player)
    {
        if (!Players.Contains(player))
            Players.Add(player);
    }

    /// <summary>
    /// Removes the given <see cref="IPlayer"/> from the pool.
    /// </summary>
    /// <param name="player"></param>
    public static void Remove(T player)
    {
        if (Players.Contains(player))
            Players.Remove(player);

        if (LocalPlayer == player)
            LocalPlayer = null;
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
    public static T GetFirst()
    {
        return (Players.Count > 0) ? Players[0] : null;
    }
    
    /// <summary>
    /// Returns the closest <see cref="IPlayer"/> in the pool to the given position.
    /// </summary>
    /// <param name="x">X position to base the check on.</param>
    /// <param name="y">Y position to base the check on.</param>
    public static T GetClosest(int x, int y)
    {
        if (Players.Count <= 1)
            return GetFirst();

        var closest = Players[0];
        var closestDistance = MathHelper.PointDistance(x, y, closest.X, closest.Y);

        for (var i = 1; i < Players.Count; i++)
        {
            if (MathHelper.PointDistance(x, y, Players[i].X, Players[i].Y) < closestDistance)
                closest = Players[i];
        }

        return closest;
    }
    
    public static T GetClosest(int x, int y, Level level)
    {
        var playersInLevel = new List<T>();

        foreach (var player in Players)
        {
            if (player.Level != level)
                continue;
            playersInLevel.Add(player);
        }

        if (playersInLevel.Count == 0)
            return null;
        
        if (playersInLevel.Count == 1)
            return playersInLevel[0];

        var closest = playersInLevel[0];
        var closestDistance = MathHelper.PointDistance(x, y, closest.X, closest.Y);

        for (var i = 1; i < playersInLevel.Count; i++)
        {
            if (MathHelper.PointDistance(x, y, playersInLevel[i].X, playersInLevel[i].Y) < closestDistance)
                closest = playersInLevel[i];
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
    public static T GetClosestOnSide(int x, int y, Side side)
    {
        if (Players.Count < 1)
            return null;
        
        T closest = null;
        var distance = 0f;
        
        foreach (var player in Players)
        {
            // Figure out if the player matches our side.
            var sideMatched = side switch
            {
                Side.Right => player.X >= x,
                Side.Down => player.Y >= y,
                Side.Left => player.X <= x,
                Side.Up => player.Y <= y,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

            // Skip if player's not on our desired side.
            if (!sideMatched)
                continue;
            
            // Get distance to player.
            var currentDistance = MathHelper.PointDistance(x, y, player.X, player.Y);
            
            // If we have a player targeted and they're closer or equal, skip.
            if (closest != null && distance <= currentDistance) 
                continue;
            
            // Update player and distance.
            closest = player;
            distance = currentDistance;
        }
        
        return closest;
    }
}