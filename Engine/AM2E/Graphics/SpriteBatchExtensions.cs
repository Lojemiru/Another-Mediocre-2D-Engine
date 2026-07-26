using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class SpriteBatchExtensions
{
    private static Matrix GetFallbackMatrix(SpriteBatch spriteBatch)
    {
        return (string)spriteBatch.Tag == "ObeyCamera" ? Camera.Transform : Matrix.Identity;
    }
    
    // Shim to provide MonoGame-like API to FNA SpriteBatch
    public static void Begin(this SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
    {
        var bs = blendState ?? BlendState.AlphaBlend;
        var ss = samplerState ?? SamplerState.PointClamp;
        var dss = depthStencilState ?? DepthStencilState.Default;
        var rs = rasterizerState ?? RasterizerState.CullCounterClockwise;
        //var tm = transformMatrix ?? (spriteBatch == Renderer.GuiBatch ? Matrix.Identity : Camera.Transform);
        var tm = transformMatrix ?? GetFallbackMatrix(spriteBatch);
        spriteBatch.Begin(sortMode, bs, ss, dss, rs, effect, tm);
    }
    
    /// <summary>
    /// Changes the <see cref="BlendState"/> that this <see cref="SpriteBatch"/> is using for rendering.
    /// WARNING: This results in a batch break! Please use sparingly.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to modify.</param>
    /// <param name="blendState">The <see cref="BlendState"/> to use.</param>
    /// <param name="samplerState">The <see cref="SamplerState"/> to use.</param>
    public static void SwapBlendState(this SpriteBatch spriteBatch, BlendState blendState, SamplerState? samplerState = null)
    {
        samplerState ??= SamplerState.PointClamp;
        spriteBatch.End();
        //spriteBatch.BeginShim(SpriteSortMode.Deferred, blendState, samplerState, transformMatrix: spriteBatch == Renderer.GuiBatch ? null : Camera.Transform);
        Begin(spriteBatch, SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, GetFallbackMatrix(spriteBatch));
    }

    /// <summary>
    /// Resets this <see cref="SpriteBatch"/>.
    /// WARNING: This results in a batch break! Please use sparingly.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to reset.</param>
    public static void ResetState(this SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        //spriteBatch.BeginShim(SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp, transformMatrix: spriteBatch == Renderer.GuiBatch ? null : Camera.Transform);
        Begin(spriteBatch, SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, GetFallbackMatrix(spriteBatch));
    }

    public static void SetShader(this SpriteBatch spriteBatch, Effect effect, BlendState? blendState = null, SamplerState? samplerState = null)
    {
        samplerState ??= SamplerState.PointClamp;
        spriteBatch.End();
        //spriteBatch.BeginShim(SpriteSortMode.Deferred, samplerState:samplerState, transformMatrix: spriteBatch == Renderer.GuiBatch ? null : Camera.Transform, effect:effect, blendState:blendState);
        Begin(spriteBatch, SpriteSortMode.Deferred, blendState, samplerState, DepthStencilState.Default, RasterizerState.CullCounterClockwise, effect, GetFallbackMatrix(spriteBatch));
    }
}