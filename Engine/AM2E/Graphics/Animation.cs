using Microsoft.Xna.Framework.Graphics;
using AM2E.Graphics;
using System;
using Microsoft.Xna.Framework;

namespace AM2E;

public sealed class Animation
{
    public Sprite Sprite { get; private set; }
    private float _index = 0;
    public int Index
    {
        get => (int)_index;
        set
        { 
            _index = value;
            WrapIndex(false);
        }
    }
        
    public float Speed { get; set; }
    
    public int Length => Sprite.Length;
    
    public int Layers => Sprite.Layers;

    public Action OnAnimationEnd = () => { };

    public string SpriteName => Sprite.Name;

    public Animation(Enum page, Enum sprite, float speed, Action onAnimationEnd = null)
        : this(page.ToString(), sprite.ToString(), speed, onAnimationEnd) { }
    
    public Animation(string page, string sprite, float speed, Action onAnimationEnd = null)
    {
        Sprite = TextureManager.GetSprite(page, sprite);
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
        return Sprite.GetAttachPoint(name, Index)[0];
    }

    public int GetAttachPointY(string name)
    {
        return Sprite.GetAttachPoint(name, Index)[1];
    }

    public Texture2D GetTexture()
    {
        return Sprite.TexturePage.Texture;
    }

    private void WrapIndex(bool runAnimEnd = true)
    {
        var exceeded = _index >= Sprite.Length;
        
        if (!exceeded && !(_index < 0)) return;
        
        var sign = exceeded ? 1 : -1;
                
        while (_index >= Sprite.Length || _index < 0)
            _index -= sign * Sprite.Length;

        if (runAnimEnd)
            OnAnimationEnd();
    }

    public void SetSprite(Enum page, Enum sprite)
        => SetSprite(page.ToString(), sprite.ToString()); 

    public void SetSprite(string page, string sprite)
    {
        Sprite = TextureManager.GetSprite(page, sprite);
        WrapIndex(false);
    }

    public void Draw(SpriteBatch spriteBatch, float x, float y, float rotation = 0, SpriteEffects effects = SpriteEffects.None, float alpha = 1, float scaleX = 1, float scaleY = 1, Color color = default, int layer = 0, bool flipOnCorner = true)
    {
        Sprite.Draw(spriteBatch, x, y, Index, rotation, effects, alpha, scaleX, scaleY, color, layer, flipOnCorner);
    }
}