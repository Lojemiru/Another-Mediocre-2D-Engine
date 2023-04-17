using System;
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

    #endregion
    
    
    #region Internal Methods

    internal static void UpdateActors()
    {
        // Step persistent actors with no layer first, then everything else by layer.
        foreach (var actor in PersistentActors.Values)
        {
            if (actor.Layer == null)
                actor.Step();
        }

        World.Tick();
    }
    
    #endregion
}