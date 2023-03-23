using System.Collections.Generic;
using AM2E.Graphics;
using AM2E.Levels;

namespace AM2E.Actors;

#region Design Notes

/*
 * Originally, this class managed a list of Actors and required each Actor to be manually registered with it.
 *      Now that Actors are managed per-layer, it only has to directly oversee persistent Actors and offer a few
 *      helper functions.
 */

#endregion

public static class ActorManager
{
    internal static readonly Dictionary<string, Actor> PersistentActors = new();
    
    
    #region Public Methods
    
    /// <summary>
    /// Check whether or not the given <see cref="Actor"/> exists.
    /// </summary>
    /// <param name="actor"></param>
    /// <returns>Whether or not the <paramref name="actor"/> exists.</returns>
    public static bool Exists(Actor actor)
    {
        return actor?.Exists ?? false;
    }
    
    public static Actor GetActor(string id)
    {
        // TODO: Rework this to do Dictionary lookups by ID instead of this crap.
        
        // Search persistent first, then in each level.
        foreach (var actor in PersistentActors.Values)
        {
            if (actor.ID == id)
                return actor;
        }

        foreach (var level in World.ActiveLevels.Values)
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
    
    // TODO: Move instantiation/removal into Actor itself.
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
        PersistentActors.Add(actor.ID, actor);
        actor.PostConstructor();
        return actor;
    }
    
    public static void RemovePersistent(string id)
    {
        PersistentActors.Remove(id);
    }
    
    public static void RemovePersistent(Actor actor)
    {
        RemovePersistent(actor.ID);
    }
    
    #endregion
    
    
    #region Internal Methods
    
    /// <summary>
    /// Deregisters all non-persistent <see cref="Actor"/>s and runs their OnRoomEnd events.
    /// </summary>
    // TODO: Shouldn't this be in Level instead?
    internal static void LevelEnd(Level level)
    {
        foreach (var actor in PersistentActors.Values)
            actor.OnLevelEnd();
        
        foreach (var layer in level.Layers.Values)
        {
            foreach (var actor in layer.Actors)
            {
                if (actor.Persistent)
                    continue;
                
                actor.OnLevelEnd();
                // TODO: Do we actually need to deregister?
                actor.Deregister();
            }
        }
    }
    
    internal static void UpdateActors()
    {
        // Step persistent actors first, then non-persistent ones
        foreach (var actor in PersistentActors.Values)
            actor.Step();
        
        foreach (var level in World.ActiveLevels.Values)
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
    
    #endregion
}