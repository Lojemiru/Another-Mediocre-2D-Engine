using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AM2E.IO;
using GameContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AM2E.Graphics;

public static class TextureManager
{
    private static ConcurrentDictionary<PageIndex, TexturePage> pages = new();
    public static Sprite InvalidSprite;
    private static readonly Texture2D invalidTexture;
    private static readonly TexturePage invalidPage;
    private static Sprite invalidSprite;

    static TextureManager()
    {   
        foreach (var page in Enum.GetValues<PageIndex>())
        {
            pages.TryAdd(page, null);
        }
        
        FileStream fileStream = new("invalidTexture.png", FileMode.Open);
        invalidTexture = Texture2D.FromStream(EngineCore._graphics.GraphicsDevice, fileStream);
        invalidPage = new TexturePage(invalidTexture);

        invalidSprite = new Sprite(1, 0, 0, new Dictionary<string, int[][]>(),
            new[] { new Rectangle(0, 0, invalidTexture.Width, invalidTexture.Height) }, invalidTexture.Width,
            invalidTexture.Height, new[] { new[] { 0, 0 } })
        {
            TexturePage = invalidPage
        };
    }

    public static bool PageExists(PageIndex index)
    {
        return pages.ContainsKey(index);
    }

    public static bool IsPageLoaded(PageIndex index)
    {
        return pages[index] != null;
    }

    public static void LoadPageBlocking(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        if (IsPageLoaded(index))
            return;

        pages[index] = TexturePage.Load(index);
    }

    public static void LoadPage(PageIndex index, Action<TexturePage> callback = null)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        if (IsPageLoaded(index))
        {
            callback?.Invoke(pages[index]);
            return;
        }

        // TODO: This will cause issues if it gets called again before the thread stops :(
        
        var t = new Thread(() =>
        {
            pages[index] = TexturePage.Load(index);
            callback?.Invoke(pages[index]);
        })
        {
            IsBackground = true
        };
        
        t.Start();
    }

    public static TexturePage GetPage(PageIndex index)
    {
        // Ensure page exists/is loaded.
        LoadPage(index);

        return pages[index];
    }

    public static void UnloadPage(PageIndex index)
    {
        if (!PageExists(index))
            throw new ArgumentException("Page does not exist: " + index);

        pages[index] = null;
        
        GC.Collect();
    }
}