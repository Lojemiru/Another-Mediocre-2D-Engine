using AM2E.Graphics;

namespace AM2E.Particles;

public struct ParticleDefinition
{
    // Rendering angle
    public float AngleMin = 0;
    public float AngleMax = 0;
    public float RotationMin = 0;
    public float RotationMax = 0;

    // Movement speed
    public float SpeedMin = 0;
    public float SpeedMax = 0;
    public float AccelMin = 0;
    public float AccelMax = 0;

    // Movement angle
    public float DirectionMin = 0;
    public float DirectionMax = 360;
    public float TurnMin = 0;
    public float TurnMax = 0;

    public readonly Sprite Sprite;

    public readonly bool SyncAngleAndDirection = false;
    
    public int IndexMin = 0;
    public int IndexMax = 0;
    public float AnimateMin = 0;
    public float AnimateMax = 0;

    public int LifetimeMin = 60;
    public int LifetimeMax = 60;

    public int AlphaMin = 1;
    public int AlphaMax = 1;
    public int FadeMin = 0;
    public int FadeMax = 0;

    public ParticleDefinition(Sprite sprite)
    {
        Sprite = sprite;
    }
}