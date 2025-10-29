using AM2E.Graphics;

namespace AM2E.Particles;

public class ParticleDefinition
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

    public bool SyncAngleAndDirection = false;
    
    public float IndexMin = 0;
    public float IndexMax = 0;
    public float AnimateMin = 0;
    public float AnimateMax = 0;

    public bool DestroyOnAnimationEnd = false;

    public float LifetimeMin = 60;
    public float LifetimeMax = 60;

    public float AlphaMin = 1;
    public float AlphaMax = 1;
    public float FadeMin = 0;
    public float FadeMax = 0;
    public float FadeDelay = 0;
    public float FadeInMin = 0;
    public float FadeInMax = 0;
    
    public float GravityDirection = 90;
    public float Gravity = 0;

    public float ScaleMin = 1f;
    public float ScaleMax = 1f;
    public float ScaleRateMin = 0;
    public float ScaleRateMax = 0;

    public ParticleDefinition(Sprite sprite)
    {
        Sprite = sprite;
    }
}