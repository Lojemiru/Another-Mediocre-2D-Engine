using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AM2E.Collision;
using AM2E.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AM2E.Actors
{
    public abstract partial class Actor : IDrawable, ICollider
    {
        
        public Actor(int x, int y, Hitbox hitbox = null)
        {
            hitbox ??= new RectangleHitbox(x, y, 16, 16);
            Collider = new Collider(hitbox);
        }

        ~Actor()
        {
            OnCleanup();
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
