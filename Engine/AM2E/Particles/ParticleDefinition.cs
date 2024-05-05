using AM2E.Graphics;

namespace AM2E.Particles;

public struct ParticleDefinition
{
    // Rendering angle
    public readonly float AngleMin = 0;
    public readonly float AngleMax = 0;
    public readonly float RotationMin = 0;
    public readonly float RotationMax = 0;

    // Movement speed
    public readonly float SpeedMin = 0;
    public readonly float SpeedMax = 0;
    public readonly float AccelMin = 0;
    public readonly float AccelMax = 0;

    // Movement angle
    public readonly float DirectionMin = 0;
    public readonly float DirectionMax = 360;
    public readonly float TurnMin = 0;
    public readonly float TurnMax = 0;

    public readonly Sprite Sprite;

    public readonly bool SyncAngleAndDirection = false;
    
    public readonly float IndexMin = 0;
    public readonly float IndexMax = 0;
    public readonly float AnimateMin = 0;
    public readonly float AnimateMax = 0;

    public readonly float LifetimeMin = 60;
    public readonly float LifetimeMax = 60;

    public readonly float AlphaMin = 1;
    public readonly float AlphaMax = 1;
    public readonly float FadeMin = 0;
    public readonly float FadeMax = 0;

    public ParticleDefinition(Sprite sprite)
    {
        Sprite = sprite;
    }
}