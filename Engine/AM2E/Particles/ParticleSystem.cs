using System;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Particles;

public sealed class ParticleSystem
{
    public readonly ParticleDefinition Definition;
    public readonly int Size;

    public int Layer
    {
        get => layer;
        set => layer = Math.Max(0, value);
    }

    private int layer = 0;
    private int index = 0;
    
    private readonly float[][] particles;

    private const int P_LIFE = 0;
    private const int P_X = 1;
    private const int P_Y = 2;
    private const int P_ANGLE = 3;
    private const int P_ROTATION = 4;
    private const int P_SPEED = 5;
    private const int P_ACCEL = 6;
    private const int P_DIRECTION = 7;
    private const int P_TURN = 8;
    private const int P_INDEX = 9;
    private const int P_ANIMATE = 10;
    private const int P_ALPHA = 11;
    private const int P_FADE = 12;
    private const int P_GRAVITY = 13;

    private const float TO_RADIANS = (float)Math.PI / 180f;

    // lifetime, x, y, angle, rotation, speed, accel, direction, turn, index, animate, alpha, fade
    private const int DATA_SCALE = 14; 
    
    public ParticleSystem(string definition, int size) 
        : this(size)
    {
        Definition = ParticleDefinitions.Definitions[definition];
    }
    
    public ParticleSystem(ParticleDefinition definition, int size) 
        : this(size)
    {
        Definition = definition;
    }

    private ParticleSystem(int size)
    {
        Size = size;
        particles = new float[size][];
        for (var i = 0; i < size; i++)
        {
            particles[i] = new float[DATA_SCALE];
        }
    }

    public void Create(float x, float y)
    {
        // TODO: Custom RNG instance so we don't interfere with the main one?
        particles[index][P_LIFE] = RNG.RandomRange(Definition.LifetimeMin, Definition.LifetimeMax);
        particles[index][P_X] = x;
        particles[index][P_Y] = y;
        particles[index][P_ANGLE] = RNG.RandomRange(Definition.AngleMin, Definition.AngleMax);
        particles[index][P_ROTATION] = RNG.RandomRange(Definition.RotationMin, Definition.RotationMax);
        particles[index][P_SPEED] = RNG.RandomRange(Definition.SpeedMin, Definition.SpeedMax);
        particles[index][P_ACCEL] = RNG.RandomRange(Definition.AccelMin, Definition.AccelMax);
        particles[index][P_DIRECTION] = RNG.RandomRange(Definition.DirectionMin, Definition.DirectionMax);
        particles[index][P_TURN] = RNG.RandomRange(Definition.TurnMin, Definition.TurnMax);
        particles[index][P_INDEX] = RNG.RandomRange(Definition.IndexMin, Definition.IndexMax);
        particles[index][P_ANIMATE] = RNG.RandomRange(Definition.AnimateMin, Definition.AnimateMax);
        particles[index][P_ALPHA] = RNG.RandomRange(Definition.AlphaMin, Definition.AlphaMax);
        particles[index][P_FADE] = RNG.RandomRange(Definition.FadeMin, Definition.FadeMax); ;
        particles[index][P_GRAVITY] = 0;
        
        index = MathHelper.Wrap(index + 1, 0, Size);
    }

    public void Update()
    {
        for (var i = 0; i < Size; i++)
        {
            var p = particles[i];
            
            if (p[P_LIFE] <= 0)
                continue;
            
            p[P_ALPHA] -= p[P_FADE];

            if (p[P_ALPHA] < 0)
            {
                p[P_LIFE] = -1;
                continue;
            }

            p[P_LIFE] -= 1;

            p[P_ANGLE] += p[P_ROTATION];
            p[P_SPEED] += p[P_ACCEL];

            if (!Definition.SyncAngleAndDirection)
                p[P_DIRECTION] += p[P_TURN];
            else
                p[P_DIRECTION] = p[P_ANGLE];

            p[P_INDEX] += p[P_ANIMATE];

            var len = Definition.Sprite.Length ;

            // If people pass massive values into this it'll break.
            // But I don't care because that would look like crap anyway and this needs to be "fast".
            if (p[P_INDEX] < 0)
            {
                if (Definition.DestroyOnAnimationEnd)
                {
                    p[P_LIFE] = -1;
                    continue;
                }
                p[P_INDEX] += len;
            }
            else if (p[P_INDEX] > len)
            {
                if (Definition.DestroyOnAnimationEnd)
                {
                    p[P_LIFE] = -1;
                    continue;
                }
                p[P_INDEX] -= len;
            }

            p[P_GRAVITY] += Definition.Gravity;

            var rads = p[P_DIRECTION] * TO_RADIANS;
            p[P_X] += MathHelper.LineComponentX(rads, p[P_SPEED]);
            p[P_Y] += MathHelper.LineComponentY(rads, p[P_SPEED]);
            
            rads = Definition.GravityDirection * TO_RADIANS;
            p[P_X] += MathHelper.LineComponentX(rads, p[P_GRAVITY]);
            p[P_Y] += MathHelper.LineComponentY(rads, p[P_GRAVITY]);
        }
    }

    public void Draw(SpriteBatch spriteBatch, float x, float y)
    {
        for (var i = 0; i < Size; i++)
        {
            var p = particles[i];
            
            if (p[P_LIFE] <= 0)
                continue;
            
            Definition.Sprite.Draw(spriteBatch, x + p[P_X], y + p[P_Y], (int)p[P_INDEX], p[P_ANGLE], SpriteEffects.None, p[P_ALPHA], layer:layer);
        }
    }
}