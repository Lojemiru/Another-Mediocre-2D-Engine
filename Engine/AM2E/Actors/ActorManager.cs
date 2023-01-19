using System;
using System.Collections.Generic;
using System.Text;
using AM2E.Actors;
using AM2E.Graphics;
using AM2E.Levels;

namespace AM2E.Actors
{
    static public class ActorManager
    {
        private static LinkedList<Actor> actors = new();

        public static Actor Instantiate(Actor actor, string layer, Level level)
        {
            RegisterActor(actor);
            level.Add(layer, actor);
            return actor;
        }

        public static Actor Instantiate(Actor actor, Layer layer, Level level)
        {
            return Instantiate(actor, layer.Name, level);
        }

        public static Actor InstantiatePersistent(Actor actor)
        {
            RegisterActor(actor);
            actor.Persistent = true;
            return actor;
        }

        public static void RegisterActor(Actor actor)
        {
            actors.AddLast(actor);
        }

        public static void DeregisterActor(Actor actor)
        {
            actors.Remove(actor);
        }

        public static void UpdateActors()
        {
            foreach (Actor actor in actors)
            {
                actor.Step();
            }
        }

        /// <summary>
        /// Deregisters all non-persistent <see cref="Actor"/>s and runs their OnRoomEnd events.
        /// </summary>
        public static void RoomEnd()
        {
            foreach (Actor actor in actors)
            {
                actor.OnRoomEnd();
                if (!actor.Persistent) actor.Deregister();
            }
        }

        /// <summary>
        /// Check whether or not the given <see cref="Actor"/> exists.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns>Whether or not the <paramref name="actor"/> exists.</returns>
        public static bool Exists(Actor actor)
        {
            return (actor == null) ? false : actor.Exists;
        }
    }
}
