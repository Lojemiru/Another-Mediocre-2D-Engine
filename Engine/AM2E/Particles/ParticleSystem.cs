using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Particles;

public sealed class ParticleSystem
{
    // We use our own RNG instance so as not to interfere with programmer usage of the static one.
    // Technically they should probably be managing their own instances if that's relevant, but I'm trying to be a
    //      little bit noob-friendly. Emphasis on "little bit."
    private static readonly RNGInstance Rng = new();
    
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
    private const int P_FADE_DELAY = 14;
    private const int P_LAYER = 15;
    private const int P_SCALE = 16;
    private const int P_SCALE_RATE = 17;
    private const int P_FADE_IN = 18;
    private const int P_FADED_IN = 19;

    private const float TO_RADIANS = (float)Math.PI / 180f;

    // lifetime, x, y, angle, rotation, speed, accel, direction, turn, index, animate, alpha, fade, gravity, fade delay, layer, scale, scale rate, fade in amount, faded in
    private const int DATA_SCALE = 20;
    
    public ParticleSystem(string definition, int size) 
        : this(size, ParticleDefinitions.Definitions[definition])
    { }
    
    public ParticleSystem(ParticleDefinition definition, int size) 
        : this(size, definition)
    { }

    private ParticleSystem(int size, ParticleDefinition definition)
    {
        Definition = definition;
        Size = size;
        particles = new float[size][];
        for (var i = 0; i < size; i++)
        {
            particles[i] = new float[DATA_SCALE];
        }
    }

    public void Create(float x, float y, int layer = -1)
    {
        particles[index][P_LIFE] = Rng.RandomRange(Definition.LifetimeMin, Definition.LifetimeMax);
        particles[index][P_X] = x;
        particles[index][P_Y] = y;
        particles[index][P_ANGLE] = Rng.RandomRange(Definition.AngleMin, Definition.AngleMax);
        particles[index][P_ROTATION] = Rng.RandomRange(Definition.RotationMin, Definition.RotationMax);
        particles[index][P_SPEED] = Rng.RandomRange(Definition.SpeedMin, Definition.SpeedMax);
        particles[index][P_ACCEL] = Rng.RandomRange(Definition.AccelMin, Definition.AccelMax);
        particles[index][P_DIRECTION] = Rng.RandomRange(Definition.DirectionMin, Definition.DirectionMax);
        particles[index][P_TURN] = Rng.RandomRange(Definition.TurnMin, Definition.TurnMax);
        particles[index][P_INDEX] = Rng.RandomRange(Definition.IndexMin, Definition.IndexMax);
        particles[index][P_ANIMATE] = Rng.RandomRange(Definition.AnimateMin, Definition.AnimateMax);
        particles[index][P_ALPHA] = Rng.RandomRange(Definition.AlphaMin, Definition.AlphaMax);
        particles[index][P_FADE] = Rng.RandomRange(Definition.FadeMin, Definition.FadeMax);
        particles[index][P_GRAVITY] = 0;
        particles[index][P_FADE_DELAY] = Definition.FadeDelay;
        particles[index][P_LAYER] = layer < 0 ? this.layer : layer;
        particles[index][P_SCALE] = Rng.RandomRange(Definition.ScaleMin, Definition.ScaleMax);
        particles[index][P_SCALE_RATE] = Rng.RandomRange(Definition.ScaleRateMin, Definition.ScaleRateMax);
        particles[index][P_FADE_IN] = Rng.RandomRange(Definition.FadeInMin, Definition.FadeInMin);
        particles[index][P_FADED_IN] = 0;
        
        index = MathHelper.Wrap(index + 1, 0, Size);
    }

    public void Update()
    {
        for (var i = 0; i < Size; i++)
        {
            var p = particles[i];
            
            if (p[P_LIFE] <= 0)
                continue;

            if (p[P_FADED_IN] == 0)
            {
                p[P_ALPHA] += p[P_FADE_IN];
                if (p[P_ALPHA] >= 1)
                    p[P_FADED_IN] = 1;
            }
            else if (p[P_FADE_DELAY] <= 0)
                p[P_ALPHA] -= p[P_FADE];

            if (p[P_ALPHA] < 0)
            {
                p[P_LIFE] = -1;
                continue;
            }

            p[P_LIFE] -= 1;
            
            if (p[P_FADED_IN] > 0)
                p[P_FADE_DELAY] -= 1;

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
            else if (p[P_INDEX] >= len)
            {
                if (Definition.DestroyOnAnimationEnd)
                {
                    p[P_LIFE] = -1;
                    continue;
                }
                p[P_INDEX] -= len;
            }

            p[P_GRAVITY] += Definition.Gravity;

            p[P_SCALE] += p[P_SCALE_RATE];

            var rads = p[P_DIRECTION] * TO_RADIANS;
            p[P_X] += MathHelper.LineComponentX(rads, p[P_SPEED]);
            p[P_Y] += MathHelper.LineComponentY(rads, p[P_SPEED]);
            
            rads = Definition.GravityDirection * TO_RADIANS;
            p[P_X] += MathHelper.LineComponentX(rads, p[P_GRAVITY]);
            p[P_Y] += MathHelper.LineComponentY(rads, p[P_GRAVITY]);
        }
    }

    public void Draw(SpriteBatch spriteBatch, float x, float y, Color color = default, bool cull = true)
    {
        for (var i = 0; i < Size; i++)
        {
            var p = particles[i];
            var pX = x + p[P_X];
            var pY = y + p[P_Y];
            var w = Definition.Sprite.Width;
            var h = Definition.Sprite.Height;
            
            if (p[P_LIFE] <= 0 || 
                (cull && (pX < Camera.BoundLeft - w || pX > Camera.BoundRight + w || pY < Camera.BoundTop - h || pY > Camera.BoundBottom + h)))
                continue;
            
            Definition.Sprite.Draw(spriteBatch, (int)pX, (int)pY, (int)p[P_INDEX], p[P_ANGLE], SpriteEffects.None, p[P_ALPHA], layer: (int)p[P_LAYER], color:color, scaleX: p[P_SCALE], scaleY: p[P_SCALE]);
        }
    }

    public void Clear()
    {
        for (var i = 0; i < Size; i++)
            particles[i][P_LIFE] = -1;
    }
}
