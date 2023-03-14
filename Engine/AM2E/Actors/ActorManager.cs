using System;
using System.Collections.Generic;
using AM2E.Graphics;
using AM2E.Levels;

namespace AM2E.Actors;

public static class ActorManager
{
    private static Dictionary<string, Actor> persistentActors = new();
    
    // TODO: Does this pattern make any sense? Now that we don't have to register them with the manager, we may not need this...
    public static Actor Instantiate(Actor actor, string layer, Level level)
    {
        level.Add(layer, actor);
        actor.PostConstructor();
        return actor;
    }

    public static Actor Instantiate(Actor actor, Layer layer, Level level)
    {
        return Instantiate(actor, layer.Name, level);
    }

    public static Actor InstantiatePersistent(Actor actor)
    {
        actor.Persistent = true;
        persistentActors.Add(actor.ID, actor);
        return actor;
    }

    public static void RemovePersistent(Actor actor)
    {
        RemovePersistent(actor.ID);
    }

    public static void RemovePersistent(string id)
    {
        persistentActors.Remove(id);
    }

    public static void UpdateActors()
    {
        // Step persistent actors first, then non-persistent ones
        foreach (var actor in persistentActors.Values)
            actor.Step();
        
        
        // TODO: Lots of layers don't have Actors. Would it significantly save performance to ignore those layers with some filtering at the Level class layer?
        // probably not lol
        foreach (var level in World.LoadedLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (var actor in layer.Actors)
                {
                    if (!actor.Persistent)
                        actor.Step();
                }
            }
        }
    }

    public static Actor GetActor(string id)
    {
        foreach (var actor in persistentActors.Values)
        {
            if (actor.ID == id)
                return actor;
        }

        foreach (var level in World.LoadedLevels.Values)
        {
            foreach (var layer in level.Layers.Values)
            {
                foreach (var actor in layer.Actors)
                {
                    if (actor.ID == id) 
                        return actor;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Deregisters all non-persistent <see cref="Actor"/>s and runs their OnRoomEnd events.
    /// </summary>
    public static void LevelEnd(Level level)
    {
        foreach (var actor in persistentActors.Values)
            actor.OnRoomEnd();
        
        foreach (var layer in level.Layers.Values)
        {
            foreach (var actor in layer.Actors)
            {
                if (actor.Persistent)
                    continue;
                
                actor.OnRoomEnd();
                // TODO: Do we actually need to deregister?
                actor.Deregister();
            }
        }
    }

    /// <summary>
    /// Check whether or not the given <see cref="Actor"/> exists.
    /// </summary>
    /// <param name="actor"></param>
    /// <returns>Whether or not the <paramref name="actor"/> exists.</returns>
    public static bool Exists(Actor actor)
    {
        return actor?.Exists ?? false;
    }
}