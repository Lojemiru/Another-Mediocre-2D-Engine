using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class SpriteBatchExtensions
{
    /// <summary>
    /// Changes the <see cref="BlendState"/> that this <see cref="SpriteBatch"/> is using for rendering.
    /// WARNING: This results in a batch break! Please use sparingly.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to modify.</param>
    /// <param name="blendState">The <see cref="BlendState"/> to use.</param>
    public static void SwapBlendState(this SpriteBatch spriteBatch, BlendState blendState)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.PointClamp, transformMatrix:Camera.Transform);
    }
    
    /// <summary>
    /// Resets the <see cref="BlendState"/> that this <see cref="SpriteBatch"/> is using for rendering.
    /// WARNING: This results in a batch break! Please use sparingly.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to reset.</param>
    public static void ResetBlendState(this SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp, transformMatrix:Camera.Transform);
    }
}