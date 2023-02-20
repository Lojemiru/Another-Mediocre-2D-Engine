﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AM2E.Collision;
using AM2E.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using AM2E.Levels;

namespace AM2E.Actors
{
    // TODO: Surely this could be made a subclass of ActorManager so that step methods etc. could be made protected...
    public abstract partial class Actor : IDrawable, ICollider
    {
        
        protected Actor(int x, int y, Hitbox hitbox = null, bool flipX = false, bool flipY = false, string id = null)
        {
            ID = id ?? Guid.NewGuid().ToString();
            Console.WriteLine(this + ": " + ID);
            hitbox ??= DefaultHitbox;
            Collider = new Collider(hitbox);
            X = x;
            Y = y;
            FlipX = flipX;
            FlipY = flipY;
            hitbox.ApplyFlips(FlipX, FlipY);
        }

        protected Actor(LDtkEntityInstance entity, int x, int y) : this(x, y, null, (entity.F & 1) != 0, (entity.F & 2) != 0, entity.Iid)
        {
        }

        ~Actor()
        {
            OnCleanup();
        }

        public virtual void PostConstructor()
        {
            // Nothing - we want an empty event so actors don't /have/ to define it.
        }

        public virtual void OnRoomStart()
        {
            // Nothing - we want an empty event so actors don't /have/ to define it.
        }

        public void Step()
        {
            OnStep();
        }

        public virtual void OnStep()
        {
            // Nothing - we want an empty event so actors don't /have/ to define it.
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            OnDraw(spriteBatch);
        }

        public virtual void OnDraw(SpriteBatch spriteBatch)
        {
            // Nothing - we want an empty draw so actors don't /have/ to define it.
        }

        public void Destroy()
        {
            OnDestroy();
            Deregister();
        }

        public virtual void OnDestroy()
        {
            // Nothing - we want an empty destroy so actors don't /have/ to define it.
        }

        public virtual void OnRoomEnd()
        {
            // Nothing - we want an empty room end so actors don't /have/ to define it.
        }

        public virtual void OnCleanup()
        {
            // Nothing - we want an empty cleanup event so actors don't /have/ to define it.
        }

        public void Deregister()
        {
            Exists = false;
            // Deregister self with ActorManager.
            ActorManager.DeregisterActor(this);
            Layer.Remove(this);
            // TODO: Does this cause the object to get automatically cleaned up?
        }
    }
}
