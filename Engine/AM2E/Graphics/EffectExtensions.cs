using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class EffectExtensions
{
    public static void StageTextureSize(this Effect effect, Texture2D texture)
    {
        effect.Parameters["TextureSize"].SetValue(new Vector2(texture.Width, texture.Height));
    }
    
    #region Parameters[parameter].SetValue(value) wrappers

    public static void SetParameter(this Effect effect, string parameter, Matrix value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Matrix[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Quaternion value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Texture value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector2 value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector2[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector3 value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector3[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector4 value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, Vector4[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, bool value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, float value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, float[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, int value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    public static void SetParameter(this Effect effect, string parameter, int[] value)
    {
        effect.Parameters[parameter].SetValue(value);
    }
    
    #endregion
}