﻿using System.Collections.Generic;
using AM2E.Levels;

namespace AM2E.Actors;

#region Design Notes

/*
 * Originally, this class managed a list of Actors and required each Actor to be manually registered with it.
 *      Now that Actors are managed per-layer, it only has to directly oversee persistent Actors and offer a few
 *      helper functions.
 *
 * Now it's doing almost nothing lol, made more sense to make things static within Actor itself.
 */

#endregion

public static class ActorManager
{
    internal static readonly Dictionary<string, Actor> PersistentActors = new();

    #region Public Methods

    

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