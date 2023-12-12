using FontStashSharp;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;

namespace AM2E.Graphics;

public static class TextHelper
{
    public static Vector2 GetAlignment(RichTextLayout rtl, HorizontalTextAlignment horizontalAlignment,
        VerticalTextAlignment verticalAlignment)
    {
        return new Vector2(GetHorizontalAlignment(rtl, horizontalAlignment), GetVerticalAlignment(rtl, verticalAlignment));
    }

    public static int GetHorizontalAlignment(RichTextLayout rtl, HorizontalTextAlignment alignment)
    {
        var x = 0;
        var size = rtl.Measure(null);
        
        switch (alignment)
        {
            case HorizontalTextAlignment.Left:
                break;
            case HorizontalTextAlignment.Center:
                x = -size.X / 2;
                break;
            case HorizontalTextAlignment.Right:
                x = -size.X;
                break;
        }

        return x;
    }

    public static int GetVerticalAlignment(RichTextLayout rtl, VerticalTextAlignment alignment)
    {
        var y = 0;
        var size = rtl.Measure(null);
        
        switch (alignment)
        {
            case VerticalTextAlignment.Top:
                break;
            case VerticalTextAlignment.Center:
                y = -size.Y / 2;
                break;
            case VerticalTextAlignment.Bottom:
                y = -size.Y;
                break;
        }

        return y;
    }

    public static Vector2 GetAlignment(string text, SpriteFontBase font,
        HorizontalTextAlignment horizontalAlignment, VerticalTextAlignment verticalAlignment,
        Vector2? scale = null,
        float characterSpacing = 0.0f,
        float lineSpacing = 0.0f,
        FontSystemEffect effect = FontSystemEffect.None,
        int effectAmount = 0)
    {
        var measure = font.MeasureString(text, scale, characterSpacing, lineSpacing, effect, effectAmount);
        return new Vector2(GetHorizontalAlignment((int)measure.X, horizontalAlignment), GetVerticalAlignment((int)measure.Y, verticalAlignment));
    }

    public static int GetHorizontalAlignment(string text, SpriteFontBase font, HorizontalTextAlignment alignment, 
        Vector2? scale = null,
        float characterSpacing = 0.0f,
        float lineSpacing = 0.0f,
        FontSystemEffect effect = FontSystemEffect.None,
        int effectAmount = 0)
    {
        var width = (int)font.MeasureString(text, scale, characterSpacing, lineSpacing, effect, effectAmount).X;

        return GetHorizontalAlignment(width, alignment);
    }

    private static int GetHorizontalAlignment(int width, HorizontalTextAlignment alignment)
    {
        var x = 0;
        
        switch (alignment)
        {
            case HorizontalTextAlignment.Left:
                break;
            case HorizontalTextAlignment.Center:
                x = -width / 2;
                break;
            case HorizontalTextAlignment.Right:
                x = -width;
                break;
        }

        return x;
    }
    
    public static int GetVerticalAlignment(string text, SpriteFontBase font, VerticalTextAlignment alignment, 
        Vector2? scale = null,
        float characterSpacing = 0.0f,
        float lineSpacing = 0.0f,
        FontSystemEffect effect = FontSystemEffect.None,
        int effectAmount = 0)
    {
        var height = (int)font.MeasureString(text, scale, characterSpacing, lineSpacing, effect, effectAmount).Y;

        return GetVerticalAlignment(height, alignment);
    }

    private static int GetVerticalAlignment(int height, VerticalTextAlignment alignment)
    {
        var y = 0;
        
        switch (alignment)
        {
            case VerticalTextAlignment.Top:
                break;
            case VerticalTextAlignment.Center:
                y = -height / 2;
                break;
            case VerticalTextAlignment.Bottom:
                y = -height;
                break;
        }

        return y;
    }
}