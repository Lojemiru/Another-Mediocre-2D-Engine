using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AM2E.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AM2E
{
    public class Animation
    {
        private Sprite sprite;
        private float _index = 0;
        public int Index
        {
            get 
            { 
                return (int)_index; 
            }
            set
            { 
                _index = value;
                WrapIndex(false);
            }
        }
        
        public float Speed { get; set; } = 1;
        public int Length
        {
            get
            {
                return sprite.Length;
            }
        }

        public Action OnAnimationEnd = () => { };

        public SpriteIndex SpriteName { get; private set; }

        public Animation(PageIndex page, SpriteIndex sprite, float speed, Action onAnimationEnd = null)
        {
            SpriteName = sprite;

            this.sprite = TextureManager.GetPage(page).Sprites[sprite];
            Speed = speed;
            OnAnimationEnd = onAnimationEnd ?? OnAnimationEnd;
        }

        public void Step()
        {
            _index += Speed;
            WrapIndex();
        }

        public int GetAttachPointX(string name)
        {
            return sprite.GetAttachPoint(name, Index)[0];
        }

        public int GetAttachPointY(string name)
        {
            return sprite.GetAttachPoint(name, Index)[1];
        }

        private void WrapIndex(bool runAnimEnd = true)
        {
            bool exceeded = _index >= sprite.Length;
            if (exceeded || _index < 0)
            {
                int sign = exceeded ? 1 : -1;
                
                while (_index >= sprite.Length || _index < 0)
                    _index -= sign * sprite.Length;

                if (runAnimEnd)
                    OnAnimationEnd();
            }
        }

        public void SetSprite(PageIndex page, SpriteIndex sprite)
        {
            SpriteName = sprite;
            this.sprite = TextureManager.GetPage(page).Sprites[sprite];
            WrapIndex(false);
        }

        public void Draw(SpriteBatch spriteBatch, int x, int y, float rotation = 0, SpriteEffects effects = SpriteEffects.None, float alpha = 1)
        {
            sprite.Draw(spriteBatch, x, y, Index, rotation, effects, alpha);
        }
    }
}
